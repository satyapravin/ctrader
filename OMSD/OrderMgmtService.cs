using Bitmex.NET;
using Bitmex.NET.Dtos;
using Bitmex.NET.Dtos.Socket;
using Bitmex.NET.Models;
using EmbeddedService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OMS.Logging;

namespace OMS
{
    public delegate void OnOrder(OrderDto o);

    public class OrderMgmtService : IEmbeddedService
    {
        #region private members
        private System.Threading.EventWaitHandle waitHandle = new System.Threading.AutoResetEvent(false);
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger(); 
        private OnOrder orderHandler = null;
        private ExchangeService.Exchange svc = null;
        private BlockingCollection<BitmexSocketDataMessage<IEnumerable<OrderDto>>> queue = new BlockingCollection<BitmexSocketDataMessage<IEnumerable<OrderDto>>>();
        private BlockingCollection<OrderDto> clientNotificationQ = new BlockingCollection<OrderDto>();
        private Dictionary<string, MyOrder> oms_cache = new Dictionary<string, MyOrder>();
        private Dictionary<string, MyOrder> oms_pending_cache = new Dictionary<string, MyOrder>();
        private Dictionary<string, MyOrder> oms_cancel_cache = new Dictionary<string, MyOrder>();
        private Dictionary<string, MyOrder> oms_amend_cache = new Dictionary<string, MyOrder>();
        private Dictionary<string, MyOrder> oms_processed_cache = new Dictionary<string, MyOrder>();
        #endregion

        #region internal methods

        internal async void AmendOrder(MyOrder req, decimal price)
        {
            MyOrder amend = new MyOrder(this);
            amend.ClientOrderID = req.Symbol + DateTime.Now.Ticks.ToString();
            amend.Request = RequestType.AMEND;
            amend.ChildOrder = req;
            amend.Symbol = req.Symbol;
            amend.Quantity = req.Quantity;
            amend.Price = price;
            amend.Side = req.Side;
            amend.Type = req.Type;
            OrderPUTRequestParams param = new OrderPUTRequestParams();
            param.ClOrdID = amend.ClientOrderID;
            param.OrigClOrdID = amend.ChildOrder.ClientOrderID;
            param.Price = amend.Price;

            lock (oms_cache)
            {
                Log.Info($"{amend.Symbol} in process for amendment with {amend.ClientOrderID} and child {req.ClientOrderID}");
                oms_amend_cache[amend.ClientOrderID] = amend;
            }

            try
            {
                await svc.Put(param).ContinueWith(AmendOrderResult, TaskContinuationOptions.NotOnFaulted);
            }
            catch(Exception e)
            {
                Log.ErrorException("amend resulted in exception", e);
                lock (oms_cache)
                {
                    oms_amend_cache.Remove(amend.ClientOrderID);
                    
                    if(oms_cache.ContainsKey(amend.ChildOrder.ClientOrderID))
                    {
                        oms_cache.Remove(amend.ChildOrder.ClientOrderID);
                    }
                }
            }
        }

        internal async void NewOrder(MyOrder req)
        {
            if (req.Status == OrderStateIdentifier.UNCONFIRMED)
            {
                OrderPOSTRequestParams param = null;

                if (req.Type == MyOrder.OrderType.LIMIT_POST)
                {
                    param = OrderPOSTRequestParams.CreateSimplePost(req.Symbol, req.Quantity, req.Price,
                                                                    req.Side == MyOrder.OrderSide.BUY ? OrderSide.Buy : OrderSide.Sell);
                }
                else if (req.Type == MyOrder.OrderType.MARKET)
                {
                    param = OrderPOSTRequestParams.CreateSimpleMarket(req.Symbol, req.Quantity,
                                                                      req.Side == MyOrder.OrderSide.BUY ? OrderSide.Buy : OrderSide.Sell);
                }
                param.ClOrdID = req.ClientOrderID;

                lock (oms_cache)
                {
                    Log.Info($"{req.Symbol} in process for submission with {req.ClientOrderID}");
                    oms_pending_cache[req.ClientOrderID] = req;
                }

                try
                {
                    await svc.Post(param).ContinueWith(NewOrderResult, TaskContinuationOptions.NotOnFaulted);
                }
                catch(Exception e)
                {
                    Log.ErrorException("New resulted in exception", e);
                    lock (oms_cache)
                    {
                        oms_pending_cache.Remove(req.ClientOrderID);
                    }
                }
            }
        }
        internal async void CancelOrder(MyOrder req)
        {
            MyOrder cancel = new MyOrder(this);
            cancel.ClientOrderID = req.Symbol + DateTime.Now.Ticks.ToString();
            cancel.Request = RequestType.CANCEL;
            cancel.ChildOrder = req;
            cancel.Symbol = req.Symbol;
            cancel.Quantity = req.Quantity;
            cancel.Price = req.Price;
            cancel.Side = req.Side;
            cancel.Type = req.Type;

            OrderDELETERequestParams param = new OrderDELETERequestParams();
            param.ClOrdID = req.ClientOrderID;

            lock (oms_cache)
            {
                Log.Info($"{cancel.Symbol} in process for cancelation with {cancel.ClientOrderID}");
                oms_cancel_cache.Add(cancel.ClientOrderID, cancel);
            }

            try
            {
                await svc.Delete(param).ContinueWith(ProcessDeleteResult, TaskContinuationOptions.NotOnFaulted);
            }
            catch (Exception e)
            {
                Log.ErrorException("Cancel resulted in exception", e);
                lock (oms_cache)
                {
                    oms_cancel_cache.Remove(cancel.ClientOrderID);
                }
            }
        }

        #endregion

        #region public methods
        public ServiceType Service { get { return ServiceType.OMS; } }
        public void RegisterHandler(string symbol, OnOrder handler)
        {
            if (orderHandler == null)
                orderHandler = handler;
            else
                orderHandler += handler;
        }
        public MyOrder GetLiveOrderForSymbol(string symbol)
        {
            MyOrder req = null;
            lock (oms_cache) { if (oms_cache.ContainsKey(symbol)) req = oms_cache[symbol]; }
            return req;
        }

        public MyOrder GetPendingOrderForSymbol(string symbol)
        {
            lock (oms_cache)
            {
                foreach (var o in oms_pending_cache.Values)
                {
                    if (o.Symbol == symbol)
                    {
                        return o;
                    }
                }

                foreach (var o in oms_amend_cache.Values)
                {
                    if (o.Symbol == symbol)
                    {
                        return o;
                    }
                }

                foreach(var o in oms_cancel_cache.Values)
                {
                    if (o.Symbol == symbol)
                    {
                        return o;
                    }
                }
            }

            return null;
        }
        public WaitHandle Start()
        {
            var thread = new Thread(new ThreadStart(OnClientNotification));
            thread.Name = "OMSThreadClient";
            thread.IsBackground = true;
            thread.Start();
            thread = new Thread(new ThreadStart(OnStart));
            thread.Name = "OMSThreadServer";
            thread.IsBackground = true;
            thread.Start();


            svc = (ExchangeService.Exchange)Locator.Instance.GetService(ServiceType.EXCHANGE);
            svc.SubscribeOrders(new ExchangeService.OnOrder(OnOrderMessage));
            return waitHandle;
        }
        public bool Stop()
        {
            queue.CompleteAdding();
            return true;
        }

        #region ordering methods
        public MyOrder NewBuyOrderMkt(string symbol, decimal qty)
        {
            return new MyOrder(this)
            {
                Request = RequestType.NEW,
                ClientOrderID = symbol + DateTime.Now.Ticks.ToString(),
                Symbol = symbol,
                Quantity = (long)qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = 0,
                Side = MyOrder.OrderSide.BUY,
                Type = MyOrder.OrderType.MARKET,
            };
        }

        public MyOrder NewSellOrderMkt(string symbol, decimal qty)
        {
            return new MyOrder(this)
            {
                Request = RequestType.NEW,
                ClientOrderID = symbol + DateTime.Now.Ticks.ToString(),
                Symbol = symbol,
                Quantity = (long)qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = 0,
                Side = MyOrder.OrderSide.SELL,
                Type = MyOrder.OrderType.MARKET,
            };
        }

        public MyOrder NewBuyOrderLimit(string symbol, decimal qty, decimal price)
        {
            return new MyOrder(this)
            {
                Request = RequestType.NEW,
                ClientOrderID = symbol + DateTime.Now.Ticks.ToString(),
                Symbol = symbol,
                Quantity = (long)qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = price,
                Side = MyOrder.OrderSide.BUY,
                Type = MyOrder.OrderType.LIMIT,
            };
        }

        public MyOrder NewSellOrderLimit(string symbol, decimal qty, decimal price)
        {
            return new MyOrder(this)
            {
                Request = RequestType.NEW,
                ClientOrderID = symbol + DateTime.Now.Ticks.ToString(),
                Symbol = symbol,
                Quantity = (long)qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = price,
                Side = MyOrder.OrderSide.SELL,
                Type = MyOrder.OrderType.LIMIT,
            };
        }

        public MyOrder NewBuyOrderPost(string symbol, decimal qty, decimal price)
        {
            return new MyOrder(this)
            {
                Request = RequestType.NEW,
                ClientOrderID = symbol + DateTime.Now.Ticks.ToString(),
                Symbol = symbol,
                Quantity = (long)qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = price,
                Side = MyOrder.OrderSide.BUY,
                Type = MyOrder.OrderType.LIMIT_POST,
            };
        }

        public MyOrder NewSellOrderPost(string symbol, decimal qty, decimal price)
        {
            return new MyOrder(this)
            {
                Request = RequestType.NEW,
                ClientOrderID = symbol + DateTime.Now.Ticks.ToString(),
                Symbol = symbol,
                Quantity = (long)qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = price,
                Side = MyOrder.OrderSide.SELL,
                Type = MyOrder.OrderType.LIMIT_POST,
            };
        }
        #endregion
        #endregion

        #region private methods

        private void TakeActionSuccess(OrderDto o)
        {
            Log.Info($"{o.Symbol} successful with {o.OrdStatus} and {o.ClOrdId}");
            lock (oms_cache)
            {
                MyOrder order = null;

                if (oms_processed_cache.ContainsKey(o.ClOrdId))
                {
                    Log.Info($"{o.ClOrdId} already processed");
                    oms_processed_cache.Remove(o.ClOrdId);
                    return;
                }

                if (oms_pending_cache.ContainsKey(o.ClOrdId))
                {
                    order = oms_pending_cache[o.ClOrdId];
                    oms_pending_cache.Remove(o.ClOrdId);
                    Log.Info($"{o.ClOrdId} removed from pending cache");
                    if (o.OrdStatus != "Canceled" && o.OrdStatus != "DoneForDay" && o.OrdStatus != "Expired" && o.OrdStatus != "Rejected")                        
                    {
                        Log.Info($"{o.ClOrdId} processed now into main cache");
                        oms_cache[order.Symbol] = order;
                    }
                    else
                    {
                        Log.Error($"Not processing new order because ordStatus is {o.OrdStatus}");
                    }
                }
                else if (oms_cancel_cache.ContainsKey(o.ClOrdId))
                {
                    order = oms_cancel_cache[o.ClOrdId];
                    oms_cancel_cache.Remove(o.ClOrdId);
                    Log.Info($"{o.ClOrdId} removed from cancel cache");
                    var child = order.ChildOrder;

                    if (child.ClientOrderID == o.ClOrdId && oms_cache.ContainsKey(order.Symbol))
                    {
                        if (o.OrdStatus != "DoneForDay" && o.OrdStatus != "Expired" && o.OrdStatus != "Rejected")
                        {
                            if (oms_cache[order.Symbol].ClientOrderID == o.ClOrdId)
                            {
                                oms_cache.Remove(o.ClOrdId);
                                Log.Info($"{o.ClOrdId} removed from main cache");
                            }
                            else
                                Log.Error($"Canceled order not a live order for {order.Symbol}");
                        }
                        else
                        {
                            Log.Error($"Not processing cancel order because ordStatus is {o.OrdStatus}");
                        }
                    }
                    else
                    {
                        Log.Info($"{child.ClientOrderID} does not match {o.ClOrdId} when processing cancel success");
                    }
                }
                else if (oms_amend_cache.ContainsKey(o.ClOrdId))
                {
                    order = oms_amend_cache[o.ClOrdId];
                    oms_amend_cache.Remove(o.ClOrdId);
                    Log.Info($"{o.ClOrdId} removed from amend cache");
                    var child = order.ChildOrder;

                    if (oms_cache.ContainsKey(child.Symbol))
                    {
                        if (o.OrdStatus != "Canceled" && o.OrdStatus != "DoneForDay" && o.OrdStatus != "Expired" && o.OrdStatus != "Rejected")
                        {
                            oms_cache[order.Symbol] = order;
                            Log.Info($"updated amended order to main cache with {order.ClientOrderID}");
                        }
                        else
                        {
                            Log.Error($"Not processing amend order because ordStatus is {o.OrdStatus}");
                        }
                    }
                    else
                    {
                        Log.Info($"{child.Symbol} not found in main cache");
                    }
                }
                else if (oms_cache.ContainsKey(o.Symbol) && oms_cache[o.Symbol].ClientOrderID == o.ClOrdId)
                {
                    order = oms_cache[o.Symbol];

                    if (o.OrdStatus == "PartiallyFilled")
                    {
                        Log.Info($"{order.ClientOrderID} partially filled");
                        order.Execution = o;
                    }
                    else if (o.OrdStatus == "Filled")
                    {
                        order.Execution = o;
                        oms_cache.Remove(o.Symbol);
                        Log.Info($"{order.ClientOrderID} filled and removed from main cache");
                    }
                    else if (o.OrdStatus == "DoneForDay" || o.OrdStatus == "Canceled" || o.OrdStatus == "Rejected" || o.OrdStatus == "Expired")
                    {
                        oms_cache.Remove(o.Symbol);
                        Log.Info($"{order.ClientOrderID} with {o.OrdStatus} removed from main cache");
                    }
                    else
                    {
                        Log.Error($"Not processing {o.OrdStatus} for order {o.ClOrdId} because it is waiting in orderbook");
                    }
                }

                if (order == null)
                {
                    Log.Error($"Order {o.ClOrdId} not found in cache for success with {o.OrdStatus}");
                }
                else
                {
                    oms_processed_cache.Add(order.ClientOrderID, order);
                    order.Status = OrderStateIdentifier.CONFIRMED;
                }
            }
        }

        private void TakeActionFailure(OrderDto o)
        {
            bool process = false;
            Log.Info($"{o.Symbol} failed with {o.OrdStatus} and {o.ClOrdId}");
            lock (oms_cache)
            {
                MyOrder order = null;

                if (oms_processed_cache.ContainsKey(o.ClOrdId))
                {
                    oms_processed_cache.Remove(o.ClOrdId);
                    return;
                }

                if (oms_pending_cache.ContainsKey(o.ClOrdId))
                {
                    order = oms_pending_cache[o.ClOrdId];
                    oms_pending_cache.Remove(o.ClOrdId);
                }
                else if (oms_cancel_cache.ContainsKey(o.ClOrdId))
                {
                    order = oms_cancel_cache[o.ClOrdId];
                    oms_cancel_cache.Remove(o.ClOrdId);
                }
                else if (oms_amend_cache.ContainsKey(o.ClOrdId))
                {
                    order = oms_amend_cache[o.ClOrdId];
                    oms_amend_cache.Remove(o.ClOrdId);
                }
                else if (oms_cache.ContainsKey(o.Symbol) && oms_cache[o.Symbol].ClientOrderID == o.ClOrdId)
                {
                    oms_cache.Remove(o.Symbol);                    
                }

                if (order == null)
                {
                    Log.Error($"Order {o.ClOrdId} not found in cache for failure with {o.OrdStatus}");
                }
                else
                {
                    if (process)
                        oms_processed_cache.Add(order.ClientOrderID, order);
                    order.Status = OrderStateIdentifier.CONFIRMED;
                }
            }
        }
    
        private void ProcessDeleteResult(Task<BitmexApiResult<List<OrderDto>>> task)
        {
            foreach (var d in task.Result.Result)
            {
                if (d.OrdStatus == "PendingCancel" || d.OrdStatus == "Canceled")
                {
                    Log.Info($"{d.OrdStatus} ACK on cancel");
                    TakeActionSuccess(d);
                }
                else if (d.OrdStatus == "Rejected")
                {
                    TakeActionFailure(d);
                    Log.Warn($"Rejection for {d.ClOrdId} on cancel with reason {d.OrdRejReason}");
                }
                else if (d.OrdStatus.ToUpper() == "INVALID ORDSTATUS")
                {
                    TakeActionFailure(d);
                    Log.Error($"Invalid order status for {d.ClOrdId} on cancel with reason {d.OrdRejReason}");
                }
                else
                {
                    Log.Warn($"Delete order request for {d.ClOrdId} with unknown ordStatus {d.OrdStatus}");
                }
            }            
        }

        private void NewOrderResult(Task<BitmexApiResult<OrderDto>> task)
        {
                var d = task.Result.Result;

            if (d.OrdStatus.ToUpper() == "INVALID ORDSTATUS")
            {
                Log.Error($"Invalid order status on new order {d.ClOrdId}");
                TakeActionFailure(d);
            }
            else if (d.OrdStatus == "Rejected")
            {
                TakeActionFailure(d);
                Log.Error($"Order rejected on new for {d.ClOrdId} with reason {d.OrdRejReason}");
            }
            else
            {
                Log.Info($"{d.ClOrdId} successful ACK on New");
                TakeActionSuccess(d);
            }            
        }

        private void AmendOrderResult(Task<BitmexApiResult<OrderDto>> task)
        {
            var d = task.Result.Result;
            if (d.OrdStatus.ToUpper() == "INVALID ORDSTATUS")
            {
                Log.Error($"Invalid order status on amend order {d.ClOrdId}");
                TakeActionFailure(d);
            }
            else if (d.OrdStatus == "Rejected")
            {
                TakeActionFailure(d);
                Log.Error($"Order rejected on amend for {d.ClOrdId} with reason {d.OrdRejReason}");
            }
            else
            {
                Log.Info($"Order ACK on amend for {d.ClOrdId}");
                TakeActionSuccess(d);
            }
            
        }
        
        private void OnOrderMessage(BitmexSocketDataMessage<IEnumerable<OrderDto>> response)
        {
            if (response.Action == BitmexActions.Partial)
            {
                lock(oms_cache)
                {
                    oms_cache.Clear();
                    oms_pending_cache.Clear();
                    oms_cancel_cache.Clear();
                    oms_amend_cache.Clear();
                }

                waitHandle.Set();
            }
            queue.Add(response);
        }
        private void OnStart()
        {
            try
            {
                foreach (var d in queue.GetConsumingEnumerable(CancellationToken.None))
                {
                    foreach (var o in d.Data)
                    {
                        if (string.IsNullOrEmpty(o.OrdStatus))
                            continue;

                        if (o.OrdStatus.ToUpper() == "INVALID ORDSTATUS" || o.OrdStatus == "Rejected")
                        {
                            TakeActionFailure(o);                            
                        }
                        else
                        {
                            TakeActionSuccess(o);
                        }
                        clientNotificationQ.Add(o);
                    }
                }
            }
            catch(Exception e)
            {
                Log.FatalException("OMS Thread crashed", e);
            }
        }

        private void OnClientNotification()
        {
            foreach (var d in clientNotificationQ.GetConsumingEnumerable(CancellationToken.None))
            {
                if (orderHandler != null)
                {
                    try
                    {
                        orderHandler(d);
                    }
                    catch(Exception e)
                    {
                        Log.ErrorException("Client order notification handler excepted", e);
                    }
                }
            }
        }
        #endregion
    }
}
