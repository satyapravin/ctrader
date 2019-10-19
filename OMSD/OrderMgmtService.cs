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

namespace OMS
{
    public delegate void OnOrder(OrderDto o);

    public class OrderMgmtService : IEmbeddedService
    {
        #region private members
        private OnOrder orderHandler = null;
        private Exchange.ExchangeService svc = null;
        private BlockingCollection<BitmexSocketDataMessage<IEnumerable<OrderDto>>> queue = new BlockingCollection<BitmexSocketDataMessage<IEnumerable<OrderDto>>>();
        private BlockingCollection<OrderDto> clientNotificationQ = new BlockingCollection<OrderDto>();
        private Dictionary<string, MyOrder> oms_cache = new Dictionary<string, MyOrder>();
        private Dictionary<string, MyOrder> oms_pending_cache = new Dictionary<string, MyOrder>();
        private Dictionary<string, MyOrder> oms_cancel_cache = new Dictionary<string, MyOrder>();
        private Dictionary<string, MyOrder> oms_amend_cache = new Dictionary<string, MyOrder>();
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
                oms_pending_cache[req.ClientOrderID] = req;
            }

            await svc.Put(param).ContinueWith(AmendOrderResult);
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
                    oms_pending_cache[req.ClientOrderID] = req;
                }

                await svc.Post(param).ContinueWith(NewOrderResult);
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
                oms_pending_cache[req.ClientOrderID] = req;
            }

            await svc.Delete(param).ContinueWith(ProcessDeleteResult);
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
        public MyOrder GetOrderForSymbol(string symbol)
        {
            MyOrder req = null;
            lock (oms_cache) { if (oms_cache.ContainsKey(symbol)) req = oms_cache[symbol]; }
            return req;
        }
        public bool Start()
        {
            var thread = new Thread(new ThreadStart(OnClientNotification));
            thread.IsBackground = true;
            thread.Start();
            thread = new Thread(new ThreadStart(OnStart));
            thread.IsBackground = true;
            thread.Start();


            svc = (Exchange.ExchangeService)Locator.Instance.GetService(ServiceType.EXCHANGE);
            svc.SubscribeOrders(new Exchange.OnOrder(OnOrderMessage));
            return true;
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
                Quantity = qty,
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
                Quantity = qty,
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
                Quantity = qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = 0,
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
                Quantity = qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = 0,
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
                Quantity = qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = 0,
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
                Quantity = qty,
                Status = OrderStateIdentifier.UNCONFIRMED,
                Price = 0,
                Side = MyOrder.OrderSide.SELL,
                Type = MyOrder.OrderType.LIMIT_POST,
            };
        }
        #endregion
        #endregion

        #region private methods
        private void EnCache(MyOrder order, RequestType requestType)
        {
            lock (oms_cache)
            {
                if (requestType == RequestType.NEW)
                    oms_pending_cache.Add(order.ClientOrderID, order);
                else if (requestType == RequestType.AMEND)
                    oms_amend_cache.Add(order.ClientOrderID, order);
                else if (requestType == RequestType.CANCEL)
                    oms_cancel_cache.Add(order.ClientOrderID, order);
            }
        }
        private void TakeActionSuccess(string cliOrdID, string ordStatus, string origCliOrdID = null)
        {
            lock (oms_cache)
            {
                MyOrder order = null;

                if (oms_pending_cache.ContainsKey(cliOrdID))
                {
                    order = oms_pending_cache[cliOrdID];
                    oms_pending_cache.Remove(cliOrdID);

                    if (ordStatus != "Canceled" && ordStatus != "DoneForDay" && ordStatus != "Expired" && ordStatus != "Rejected")                        
                    {
                        oms_cache[order.Symbol] = order;
                    }
                }
                else if (oms_cancel_cache.ContainsKey(cliOrdID))
                {
                    order = oms_cancel_cache[cliOrdID];
                    oms_cancel_cache.Remove(cliOrdID);
                    var child = order.ChildOrder;
                    if (child.ClientOrderID == cliOrdID && oms_cache.ContainsKey(order.Symbol))
                    {
                        if (ordStatus != "DoneForDay" && ordStatus != "Expired" && ordStatus != "Rejected")
                        {
                            if (oms_cache[order.Symbol].ClientOrderID == cliOrdID)
                                oms_cache.Remove(cliOrdID);
                            else
                                System.Console.WriteLine("Canceled order not a live order for symbol " + order.Symbol);
                        }
                    }
                }
                else if (oms_amend_cache.ContainsKey(cliOrdID))
                {
                    order = oms_amend_cache[cliOrdID];
                    oms_amend_cache.Remove(cliOrdID);
                    var child = order.ChildOrder;

                    if (oms_cache.ContainsKey(child.Symbol) && origCliOrdID != null && origCliOrdID == child.ClientOrderID)
                    {
                        if (ordStatus != "Canceled" && ordStatus != "DoneForDay" && ordStatus != "Expired" && ordStatus != "Rejected")
                        {
                            oms_cache[order.Symbol] = order;
                        }
                    }
                }

                if (order == null)
                {
                    System.Console.WriteLine("Order not found in cache for success " + cliOrdID);
                }
                else
                {
                    order.Status = OrderStateIdentifier.CONFIRMED;
                }
            }
        }

        private void TakeActionFailure(string cliOrdID)
        {
            lock (oms_cache)
            {
                MyOrder order = null;

                if (oms_pending_cache.ContainsKey(cliOrdID))
                {
                    order = oms_pending_cache[cliOrdID];
                    oms_pending_cache.Remove(cliOrdID);
                }
                else if (oms_cancel_cache.ContainsKey(cliOrdID))
                {
                    order = oms_cancel_cache[cliOrdID];
                    oms_cancel_cache.Remove(cliOrdID);
                }
                else if (oms_amend_cache.ContainsKey(cliOrdID))
                {
                    order = oms_amend_cache[cliOrdID];
                    oms_amend_cache.Remove(cliOrdID);
                }

                if (order == null)
                {
                    System.Console.WriteLine("Order not found in cache for success " + cliOrdID);
                }
                else
                {
                    order.Status = OrderStateIdentifier.CONFIRMED;
                }
            }
        }
    
        private void ProcessDeleteResult(Task<BitmexApiResult<List<OrderDto>>> task)
        {
            if (task.Exception != null)
            {
                Console.WriteLine((task.Exception.InnerException ?? task.Exception).Message);
            }
            else
            {
                foreach (var d in task.Result.Result)
                {
                    if (d.OrdStatus == "PendingCancel" || d.OrdStatus == "Canceled")
                    {
                        TakeActionSuccess(d.ClOrdId, d.OrdStatus);
                    }
                    else if (d.OrdStatus == "Rejected")
                    {
                        TakeActionFailure(d.ClOrdId);
                        Console.WriteLine("Rejection for symbol on cancel " + d.Symbol + " with reason " + d.OrdRejReason);
                    }
                    else if (d.OrdStatus.ToUpper() == "INVALID ORDSTATUS")
                    {
                        TakeActionFailure(d.ClOrdId);
                        Console.WriteLine("Invalid order status for symbol on cancel " + d.Symbol + " with reason " + d.OrdRejReason);
                    }
                }
            }
        }

        private void NewOrderResult(Task<BitmexApiResult<OrderDto>> task)
        {
            if (task.Exception != null)
            {
                Console.WriteLine((task.Exception.InnerException ?? task.Exception).Message);
            }
            else
            {
                var d = task.Result.Result;

                if (d.OrdStatus.ToUpper() == "INVALID ORDSTATUS")
                {
                    Console.WriteLine("Invalid order status on new order");
                    TakeActionFailure(d.ClOrdId);
                }
                else
                {
                    TakeActionSuccess(d.ClOrdId, d.OrdStatus);
                }
            }
        }

        private void AmendOrderResult(Task<BitmexApiResult<OrderDto>> task)
        {
            if (task.Exception != null)
            {
                Console.WriteLine((task.Exception.InnerException ?? task.Exception).Message);
            }
            else
            {
                var d = task.Result.Result;
                if (d.OrdStatus.ToUpper() == "INVALID ORDSTATUS")
                {
                    Console.WriteLine("Invalid order status on amend order");
                    TakeActionFailure(d.ClOrdId);
                }
                else if (d.OrdStatus == "Rejected")
                {
                    TakeActionFailure(d.ClOrdId);
                    Console.WriteLine("Order rejected on amend for symbol " + d.Symbol + " with reason " + d.OrdRejReason);
                }
                else
                {
                    TakeActionSuccess(d.ClOrdId, d.OrdStatus);
                }
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
            }

            queue.Add(response);
        }
        private void OnStart()
        {
            foreach (var d in queue.GetConsumingEnumerable(CancellationToken.None))
            {
                OnOrderMessage(d);

                foreach(var o in d.Data)
                {
                    clientNotificationQ.Add(o);
                }
            }
        }

        private void OnClientNotification()
        {
            foreach (var d in clientNotificationQ.GetConsumingEnumerable(CancellationToken.None))
            {
                if (orderHandler != null)
                {
                    orderHandler(d);
                }
            }
        }
        #endregion
    }
}
