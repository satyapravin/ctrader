

using System;
using System.Threading;
using OMS;
using MDS;
using PMS;
using EmbeddedService;
using Executor.Logging;

namespace Executor
{
    public class MarketMaker
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        public string Symbol { get { return prop.Symbol(); } }
        private InstrProp prop;
        private volatile bool breakLoop = false;
        private decimal positionValue = 0;
        private decimal targetPrice = 0;
        private bool chaseM = true;

        public MarketMaker(InstrProp prop)
        {
            this.prop = prop;
        }

        public void Start()
        {
            var thread = new Thread(new ThreadStart(OnStart));
            thread.Name = "MM-" + Symbol;
            thread.IsBackground = true;
            thread.Start();
        }

        public void FillTarget(decimal positionValue, decimal avgPrice, bool chase = true)
        {
            lock(this)
            {
                this.positionValue = positionValue;
                this.targetPrice = avgPrice;
                this.chaseM = chase;
            }
        }

        private void OnStart()
        {
            try
            {
                var oSvc = (OrderMgmtService)Locator.Instance.GetService(ServiceType.OMS);
                var mSvc = (MarketDataService)Locator.Instance.GetService(ServiceType.MDS);
                var pSvc = (PositionService)Locator.Instance.GetService(ServiceType.PMS);

                while (!breakLoop)
                {
                    decimal bidPrice = -1;
                    decimal askPrice = -1;
                    bool chase = true;
                    decimal pval = 0;
                    decimal avgp = 0;

                    lock (this)
                    {
                        bidPrice = mSvc.GetBestBid(Symbol);
                        askPrice = mSvc.GetBestAsk(Symbol);
                        chase = chaseM;
                        pval = positionValue;
                        avgp = targetPrice;
                    }

                    if (bidPrice > 0 && askPrice > 0)
                    {
                        decimal currentQty = pSvc.GetQuantity(Symbol);
                        var posval = prop.GetPositionValue(currentQty, bidPrice, askPrice);
                        var delta = positionValue - posval;
                        var deltaQ = prop.GetQuantity(delta, bidPrice, askPrice);
                        var liveReq = oSvc.GetLiveOrderForSymbol(Symbol);
                        var pendingReq = oSvc.GetPendingOrderForSymbol(Symbol);

                        if (pendingReq == null)
                        {
                            if (deltaQ > 0)
                            {
                                if (liveReq == null)
                                {
                                    if (chase)
                                    {
                                        liveReq = oSvc.NewBuyOrderPost(Symbol, deltaQ, bidPrice);
                                    }
                                    else
                                    {
                                        liveReq = oSvc.NewBuyOrderMkt(Symbol, deltaQ);
                                    }

                                    liveReq.Send();
                                }
                                else
                                {
                                    liveReq.Cancel();
                                }
                            }
                            else if (deltaQ < 0)
                            {
                                if (liveReq == null)
                                {
                                    if (chase)
                                    {
                                        liveReq = oSvc.NewSellOrderPost(Symbol, -deltaQ, askPrice);
                                    }
                                    else
                                    {
                                        liveReq = oSvc.NewSellOrderMkt(Symbol, -deltaQ);
                                    }
                                    liveReq.Send();
                                }
                                else
                                {
                                    liveReq.Cancel();
                                }
                            }
                            else
                            {
                                if (liveReq != null)
                                {
                                    if (chase)
                                    {
                                        if (liveReq.Side == MyOrder.OrderSide.BUY && bidPrice != liveReq.Price)
                                            liveReq.Amend(bidPrice);
                                        else if (liveReq.Side == MyOrder.OrderSide.SELL && askPrice != liveReq.Price)
                                            liveReq.Amend(askPrice);
                                    }
                                }

                            }
                        }
                    }

                    Thread.Sleep(2500);
                }
            }
            catch (Exception e)
            {
                Log.FatalException(Symbol, e);
            }
        }

        public void Stop()
        {
            breakLoop = true;
        }
    }
}
