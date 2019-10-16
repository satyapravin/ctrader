

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
        public string Symbol { get; }
        private volatile bool breakLoop = false;
        private long inventory = 0;
        private bool chaseM = true;

        public MarketMaker(string symbol)
        {
            Symbol = symbol;
        }

        public void Start()
        {
            var thread = new Thread(new ThreadStart(OnStart));
            thread.IsBackground = true;
            thread.Start();
        }

        public void FillTarget(long invtry, bool chase = true)
        {
            lock(this)
            {
                this.inventory = invtry;
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
                long qty = 0;

                lock(this)
                {
                    bidPrice = mSvc.GetBestBid(Symbol); 
                    askPrice = mSvc.GetBestAsk(Symbol);
                    chase = chaseM;
                    qty = inventory;
                }

                if (bidPrice > 0 && qty != 0 && askPrice > 0)
                {
                    long currentPosition = pSvc.GetQuantity(Symbol);
                    var delta = qty - currentPosition;
                    var req = oSvc.GetOrderForSymbol(Symbol);

                    if (delta > 0)
                    {
                        if (req == null)
                        {
                            if (chase)
                            {
                                req = oSvc.NewBuyOrderPost(Symbol, delta, bidPrice);
                            }
                            else
                            {
                                req = oSvc.NewBuyOrderMkt(Symbol, delta);
                            }
                            req.Send();
                        }
                        else
                        {
                            if (chase) req.Amend(delta, bidPrice);
                        }
                    }
                    else if (delta < 0)
                    {
                        if (req == null)
                        {
                            if (chase)
                            {
                                req = oSvc.NewSellOrderPost(Symbol, -delta, askPrice);
                            }
                            else
                            {
                                req = oSvc.NewSellOrderMkt(Symbol, -delta);
                            }
                            req.Send();
                        }
                        else
                        {
                            if (chase) req.Amend(delta, askPrice);
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
