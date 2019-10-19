

using System;
using System.Threading;
using OMS;
using MDS;
using PMS;
using EmbeddedService;

namespace Executor
{
    public class MarketMaker
    {
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
            var oSvc = (OrderMgmtService)Locator.Instance.GetService(ServiceType.OMS);
            var mSvc = (MarketDataService)Locator.Instance.GetService(ServiceType.MDS);
            var pSvc = (PositionService)Locator.Instance.GetService(ServiceType.PMS);

            while(!breakLoop)
            {
                decimal bidPrice = -1;
                decimal askPrice = -1;
                bool chase = true;
                decimal pval = 0;
                decimal avgp = 0;

                lock(this)
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
                    var req = oSvc.GetOrderForSymbol(Symbol);

                    if (deltaQ > 0)
                    {
                        if (req == null)
                        {
                            if (chase)
                            {
                                req = oSvc.NewBuyOrderPost(Symbol, deltaQ, bidPrice);
                            }
                            else
                            {
                                req = oSvc.NewBuyOrderMkt(Symbol, deltaQ);
                            }

                            req.Send();
                        }
                        else
                        {
                            req.Cancel();
                        }
                    }
                    else if (deltaQ < 0)
                    {
                        if (req == null)
                        {
                            if (chase)
                            {
                                req = oSvc.NewSellOrderPost(Symbol, -deltaQ, askPrice);
                            }
                            else
                            {
                                req = oSvc.NewSellOrderMkt(Symbol, -deltaQ);
                            }
                            req.Send();
                        }
                        else
                        {
                            req.Cancel();
                        }
                    }
                    else
                    {
                        if (req != null)
                        {
                            if (chase)
                            {
                                if (req.Side == MyOrder.OrderSide.BUY && bidPrice != req.Price)
                                    req.Amend(bidPrice);
                                else if (req.Side == MyOrder.OrderSide.SELL && askPrice != req.Price)
                                    req.Amend(askPrice);
                            }
                        }

                    }
                }

                Thread.Sleep(2500);
            }
        }

        public void Stop()
        {
            breakLoop = true;
        }
    }
}
