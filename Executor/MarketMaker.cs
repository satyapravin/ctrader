

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
        private double positionValue = 0;
        private double targetPrice = 0;
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

        public void FillTarget(double positionValue, double avgPrice, bool chase = true)
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
            var oSvc = (OrderMgmtService)Locator.Instance.GetService("OrderMgmtService");
            var mSvc = (MarketDataService)Locator.Instance.GetService("MarketDataService");
            var pSvc = (PositionService)Locator.Instance.GetService("PositionService");

            while(!breakLoop)
            {
                double bidPrice = -1;
                double askPrice = -1;
                bool chase = true;
                double pval = 0;
                double avgp = 0;

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
                    long currentPosition = pSvc.GetQuantity(Symbol);
                    var posval = prop.GetPositionValue(currentPosition, bidPrice, askPrice);
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
                            if (chase)
                            {
                                req.Amend(deltaQ, bidPrice);
                            }
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
                            if (chase)
                            {
                                req.Amend(deltaQ, askPrice);
                            }
                        }
                    }
                    else
                    {
                        if (req != null)
                        {
                            if (chase)
                            {
                                if (req.isBuy && bidPrice != req.price)
                                    req.Amend(0, bidPrice);
                                else if (!req.isBuy && askPrice != req.price)
                                    req.Amend(0, askPrice);
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
