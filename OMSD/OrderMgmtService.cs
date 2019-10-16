using Bitmex.Client.Websocket.Responses.Executions;
using Bitmex.Client.Websocket.Responses.Orders;
using EmbeddedService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OMS
{
    public enum OrderRequestStatus { Undefined, New, Cancel, PartialFill, Fill, Rejected };
    public class OrderNotifiction
    {
        public string clOrdID = string.Empty;
        public OrderStatus status = OrderStatus.Undefined;
    }
    public class OrderNew : OrderNotifiction
    {
    }

    public class OrderCanceled : OrderNotifiction
    {
    }

    public class OrderPartiallyFilled : OrderNotifiction
    {
        public long cumQty = 0;
        public double avgPx = 0;
    }

    public class OrderFilled : OrderNotifiction
    {
        public long Qty = 0;
        public double avgPx = 0;
    }

    public class OrderRejected : OrderNotifiction
    {
        public string reason = string.Empty;
    }

    public class OrderRequest
    {
        public OrderRequest(OrderMgmtService svc)
        {
            this.oms_server = svc;
        }

        public bool Send()
        {
            if (!sent)
            {
                sent = oms_server.NewOrder(this);
            }

            return sent;
        }

        public bool Cancel()
        {
            if (sent && !canceled)
            {
                canceled = oms_server.CancelOrder(this);
            }

            return canceled;
        }

        public OrderRequest Amend(long delta, double price)
        {
            OrderRequest retval = null;

            if (sent && !canceled)
            {
                if (delta == 0 && price != this.price)
                {
                   return oms_server.AmendOrder(this, this.quantity, price);
                }

                var qty = this.isBuy ? this.quantity : -this.quantity;
                long newqty = qty + delta;

                if (this.isBuy)
                {
                    if (newqty < 0)
                    {
                        this.Cancel();
                    }
                    else if (newqty > 0)
                    {
                        oms_server.AmendOrder(this, newqty, price);
                    }
                    else
                    {
                        this.Cancel();
                    }
                }
                else
                {
                    if (newqty > 0)
                    {
                        this.Cancel();
                    }
                    else if (newqty < 0)
                    {
                        oms_server.AmendOrder(this, Math.Abs(newqty), price);
                    }
                    else
                    {
                        this.Cancel();
                    }
                }
            }

            return retval;
        }


        public void Copy(OrderRequest req)
        {
            lock (req)
            {
                this.ordStatus = req.ordStatus;
                this.exceptionString = req.exceptionString;
                this.sent = req.sent;
                this.canceled = req.canceled;
                this.symbol = req.symbol;
                this.isBuy = req.isBuy;
                this.clOrdID = req.clOrdID;
                this.OriginalclOrdID = req.OriginalclOrdID;
                this.isLimit = req.isLimit;
                this.postOnly = req.postOnly;
                this.quantity = req.quantity;
                this.price = req.price;
                this.previous = req.previous;
                this.avgPx = req.avgPx;
                this.cumQty = req.cumQty;
            }
        }

        public OrderRequestStatus ordStatus = OrderRequestStatus.Undefined;
        public string exceptionString = string.Empty;
        private bool sent = false;
        private bool canceled = false;
        private OrderMgmtService oms_server = null;
        public string symbol = string.Empty;
        public bool isBuy = false;
        public string clOrdID = string.Empty;
        public string OriginalclOrdID = string.Empty;
        public bool isLimit = true;
        public bool postOnly = true;
        public long quantity = 0;
        public double price = 0;
        public OrderRequest previous = null;
        public double avgPx = 0;
        public long cumQty = 0;
    }

    public delegate void OnOrderAck(OrderNew ntfy, OrderRequest r);
    public delegate void OnOrderCanceled(OrderCanceled ntfy, OrderRequest r);
    public delegate void OnOrderPartiallyFilled(OrderPartiallyFilled ntfy, OrderRequest r);
    public delegate void OnOrderFilled(OrderFilled ntfy, OrderRequest r);
    public delegate void OnOrderRejected(OrderRejected ntfy, OrderRequest r);

    public class OrderMgmtService : IEmbeddedService
    {
        private BitMex.BitMexService svc = null;
        private BlockingCollection<Order> queue = new BlockingCollection<Order>();
        private ConcurrentDictionary<string, OrderRequest> oms_cache = new ConcurrentDictionary<string, OrderRequest>();
        private ConcurrentDictionary<string, 
                Tuple<OnOrderAck, 
                    OnOrderCanceled, 
                    OnOrderFilled, 
                    OnOrderPartiallyFilled, 
                    OnOrderRejected>> symbolHandlers = new ConcurrentDictionary<string, Tuple<OnOrderAck, 
                                                                                              OnOrderCanceled, 
                                                                                              OnOrderFilled, 
                                                                                              OnOrderPartiallyFilled, 
                                                                                              OnOrderRejected>>();

        public void RegisterHandler(string symbol, Tuple<OnOrderAck, OnOrderCanceled, OnOrderFilled, OnOrderPartiallyFilled, OnOrderRejected> tpl)
        {
            symbolHandlers[symbol] = tpl;
        }

        public OrderRequest GetOrderForSymbol(string symbol)
        {
            OrderRequest req = null;
            oms_cache.TryGetValue(symbol, out req);
            return req;
        }
        public void Start()
        {
            var thread = new Thread(new ThreadStart(OnStart));
            thread.IsBackground = true;
            thread.Start();

            svc = (BitMex.BitMexService)Locator.Instance.GetService("BitMexService");
            svc.SubscribeOrders(new BitMex.OnOrder(OnOrderMessage));
            //svc.SubscribeExecutions(new BitMex.OnExecution(OnExecutionMessage)); // do we need this now?
        }

        public OrderRequest NewBuyOrderMkt(string symbol, long qty)
        {
            return new OrderRequest(this)
            {
                clOrdID = symbol + DateTime.Now.Ticks.ToString(),
                symbol = symbol,
                quantity = qty,
                isBuy = true,
                isLimit = false,
                postOnly = false
            };
        }

        public OrderRequest NewSellOrderMkt(string symbol, long qty)
        {
            return new OrderRequest(this)
            {
                clOrdID = symbol + DateTime.Now.Ticks.ToString(),
                symbol = symbol,
                quantity = qty,
                isBuy = false,
                isLimit = false,
                postOnly = false
            };
        }

        public OrderRequest NewBuyOrderLimit(string symbol, long qty, double price)
        {
            return new OrderRequest(this)
            {
                clOrdID = symbol + DateTime.Now.Ticks.ToString(),
                symbol = symbol,
                quantity = qty,
                price = price,
                isBuy = true,
                isLimit = true,
                postOnly = false
            };
        }

        public OrderRequest NewSellOrderLimit(string symbol, long qty, double price)
        {
            return new OrderRequest(this)
            {
                clOrdID = symbol + DateTime.Now.Ticks.ToString(),
                symbol = symbol,
                quantity = qty,
                price = price,
                isBuy = false,
                isLimit = true,
                postOnly = false
            };
        }

        public OrderRequest NewBuyOrderPost(string symbol, long qty, double price)
        {
            return new OrderRequest(this)
            {
                clOrdID = symbol + DateTime.Now.Ticks.ToString(),
                symbol = symbol,
                quantity = qty,
                price = price,
                isBuy = true,
                isLimit = true,
                postOnly = true
            };
        }

        public OrderRequest NewSellOrderPost(string symbol, long qty, double price)
        {
            return new OrderRequest(this)
            {
                clOrdID = symbol + DateTime.Now.Ticks.ToString(),
                symbol = symbol,
                quantity = qty,
                price = price,
                isBuy = false,
                isLimit = true,
                postOnly = true
            };
        }

        internal bool NewOrder(OrderRequest req)
        {
            if (oms_cache.ContainsKey(req.symbol))
                throw new ApplicationException("Cannot have simultaneous orders on same symbol");
            oms_cache[req.symbol] = req;
            var param = new Dictionary<string, object>();
            param["symbol"] = req.symbol;
            param["side"] = req.isBuy ? "Buy" : "Sell";
            param["orderQty"] = req.quantity;
            param["ordType"] = req.isLimit ? "Limit" : "Market";
            param["clOrdID"] = req.clOrdID;
            if (req.isLimit)
            {
                param["price"] = req.price;
            }

            if (req.postOnly)
            {
                param["execInst"] = "ParticipateDoNotInitiate";
            }

            var retval = svc.Query("POST", "/order", param, true);
            return true;
        }

        internal OrderRequest AmendOrder(OrderRequest req, long qty, double price)
        {
            OrderRequest amend = new OrderRequest(this);
            amend.Copy(req);
            amend.exceptionString = string.Empty;
            amend.OriginalclOrdID = req.clOrdID;
            amend.clOrdID = req.symbol + DateTime.Now.Ticks.ToString();
            amend.quantity = qty;
            amend.price = price;
            amend.previous = req;
            amend.ordStatus = OrderRequestStatus.Undefined;

            long leavesQty = 0;

            if (req.ordStatus == OrderRequestStatus.PartialFill)
            {
                leavesQty = qty - req.cumQty;
            }

            oms_cache[req.symbol] = amend;

            var param = new Dictionary<string, object>();
            param["orderQty"] = amend.quantity;
            param["clOrdID"] = amend.clOrdID;
            param["price"] = amend.price;
            param["origClOrdID"] = amend.OriginalclOrdID;

            if (req.ordStatus == OrderRequestStatus.PartialFill)
                param["leavesQty"] = leavesQty;

            var retval = svc.Query("PUT", "/order", param, true);
            return amend;
        }

        internal bool CancelOrder(OrderRequest req)
        {
            var param = new Dictionary<string, object>();
            lock (req)
            {
                param["clOrdID"] = req.clOrdID;
            }
            var retval = svc.Query("DELETE", "/order", param, true);
            return true;
        }

        void OnOrderMessage(OrderResponse response)
        {
            for (int ii = 0; ii < response.Data.Length; ii++)
            {
                queue.Add(response.Data[ii]);                
            }
        }

        void OnExecutionMessage(ExecutionResponse response)
        {
            throw new NotImplementedException("Should not subscribe to execution messages");
        }

        void OnStart()
        {
            foreach (var d in queue.GetConsumingEnumerable(CancellationToken.None))
            {
                if (d.OrdStatus == OrderStatus.New)
                {
                    if (oms_cache.ContainsKey(d.Symbol))
                    {
                        var req = oms_cache[d.Symbol];
                        lock (req)
                        {
                            req.ordStatus = OrderRequestStatus.New;
                        }

                        if (symbolHandlers.ContainsKey(d.Symbol))
                            symbolHandlers[d.Symbol].Item1(new OrderNew() { clOrdID = d.ClOrdId, status = d.OrdStatus }, req);
                    }
                }

                if (d.OrdStatus == OrderStatus.Canceled)
                {
                    if (oms_cache.ContainsKey(d.Symbol))
                    {
                        OrderRequest req = null;
                        oms_cache.Remove(d.Symbol, out req);

                        lock (req)
                        {
                            req.ordStatus = OrderRequestStatus.Cancel;
                        }

                        if (symbolHandlers.ContainsKey(d.Symbol))
                            symbolHandlers[d.Symbol].Item2(new OrderCanceled() { clOrdID = d.ClOrdId, status = d.OrdStatus }, req);
                    }
                }

                if (d.OrdStatus == OrderStatus.Filled)
                {
                    if (oms_cache.ContainsKey(d.Symbol))
                    {
                        OrderRequest req = null;
                        oms_cache.Remove(d.Symbol, out req);
                        lock (req)
                        {
                            req.ordStatus = OrderRequestStatus.Fill;
                        }

                        if (symbolHandlers.ContainsKey(d.Symbol))
                            symbolHandlers[d.Symbol].Item3(new OrderFilled() { clOrdID = d.ClOrdId, status = d.OrdStatus, Qty = d.OrderQty.HasValue ? d.OrderQty.Value : 0 }, req);
                    }
                }

                if (d.OrdStatus == OrderStatus.PartiallyFilled)
                {
                    if (oms_cache.ContainsKey(d.Symbol))
                    {
                        OrderRequest req = oms_cache[d.Symbol];

                        lock (req)
                        {
                            req.ordStatus = OrderRequestStatus.PartialFill;
                            req.cumQty = d.CumQty.HasValue ? d.CumQty.Value : 0;
                            req.avgPx = d.AvgPx.HasValue ? d.AvgPx.Value : 0;
                        }

                        if (symbolHandlers.ContainsKey(d.Symbol))
                            symbolHandlers[d.Symbol].Item4(new OrderPartiallyFilled() { clOrdID = d.ClOrdId, status = d.OrdStatus, cumQty = d.CumQty.HasValue ? d.CumQty.Value : 0, avgPx = d.AvgPx.HasValue ? d.AvgPx.Value : 0 }, req);
                    }
                }

                if (d.OrdStatus == OrderStatus.Rejected)
                {
                    if (oms_cache.ContainsKey(d.Symbol))
                    {
                        OrderRequest req = null;
                        oms_cache.Remove(d.Symbol, out req);

                        if (req.previous != null)
                        {
                            oms_cache[d.Symbol] = req.previous;
                        }
                        
                        lock (req)
                        {
                            req.ordStatus = OrderRequestStatus.Rejected;
                            req.exceptionString = d.OrdRejReason;
                        }

                        if (symbolHandlers.ContainsKey(d.Symbol))
                            symbolHandlers[d.Symbol].Item5(new OrderRejected() { clOrdID = d.ClOrdId, status = d.OrdStatus, reason = d.OrdRejReason }, req);
                    }
                }
            }
        }

        public bool Stop()
        {
            queue.CompleteAdding();
            return true;
        }
    }
}
