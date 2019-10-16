using EmbeddedService;
using System;
using System.Collections.Generic;
using System.Configuration;
using BitMex;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.IO;
using System.Threading;
using MDS;
using PMS;
using MGS;
using OMS;
using Bitmex.Client.Websocket.Utils;

namespace CTrader
{
    public class Program
    {
        private static void InitLogging()
        {
            var executingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var logPath = Path.Combine(executingDir, "logs", "verbose.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void OnOrderAck(OMS.OrderNew ntfy, OrderRequest req)
        {

        }

        public static void OnOrderCanceld(OMS.OrderCanceled ntfy, OrderRequest req)
        {

        }

        public static void OnOrderFilled(OMS.OrderFilled ntfy, OrderRequest req)
        {
            avgPx = ntfy.avgPx;

            filledQty = ntfy.Qty;

            if (!req.isBuy)
                filledQty *= -1;
        }

        public static void OnOrderPartialFilled(OMS.OrderPartiallyFilled ntfy, OrderRequest req)
        {
            avgPx = ntfy.avgPx;
            filledQty = ntfy.cumQty;

            if (!req.isBuy)
                filledQty *= -1;
        }

        public static void OnOrderRejected(OMS.OrderRejected ntfy, OrderRequest req)
        {

        }

        public static double avgPx = 0;
        public static long filledQty = 0;

        public static void Main()
        {
            Console.WriteLine("Starting CTrader");
            InitLogging();
            BitMexService bitmexSvc = new BitMexService(System.Configuration.ConfigurationManager.AppSettings["ApiKey"],
                                                        System.Configuration.ConfigurationManager.AppSettings["ApiSecret"],
                                                        bool.Parse(System.Configuration.ConfigurationManager.AppSettings["IsLive"]));

            MarketDataService mdsSvc = new MarketDataService();
            PositionService pSvc = new PositionService();
            OrderMgmtService omsSvc =  new OrderMgmtService();
            MarginService mgsSvc = new MarginService();

            Locator.Instance.Register("BitMexService", bitmexSvc);
            Locator.Instance.Register("MarketDataService", mdsSvc);
            Locator.Instance.Register("PositionService", pSvc);
            Locator.Instance.Register("OrderMgmtService", omsSvc);
            Locator.Instance.Register("MarginService", mgsSvc);
            List<string> symbols = new List<string>();
            symbols.Add("XBTUSD");
            symbols.Add("ETHUSD");
            symbols.Add("ETHZ19");

            omsSvc.RegisterHandler("ETHZ19", new Tuple<OnOrderAck, OnOrderCanceled, OnOrderFilled, OnOrderPartiallyFilled, OnOrderRejected>(OnOrderAck, OnOrderCanceld, OnOrderFilled, OnOrderPartialFilled, OnOrderRejected));
            mdsSvc.Register(symbols);
            mdsSvc.Start();
            pSvc.Start();
            mgsSvc.Start();
            omsSvc.Start();
            bitmexSvc.Start();
            Executor.MarketMaker xcfmaker = new Executor.MarketMaker("ETHZ19");
            Executor.MarketMaker btcmaker = new Executor.MarketMaker("XBTUSD");
            Executor.MarketMaker ethmaker = new Executor.MarketMaker("ETHUSD");

            ethmaker.Start();
            btcmaker.Start();
            xcfmaker.Start();

            long counter = 0;
            while (true)
            {
                counter++;
                var amount = mgsSvc.Amount * 0.75;
                amount *= 10;
                var margin = mgsSvc.AvailableMargin * 0.7;
                margin *= 10;

                var btcAsk = mdsSvc.GetBestAsk("XBTUSD");
                var ethAsk = mdsSvc.GetBestAsk("ETHUSD");
                var xcfAsk = mdsSvc.GetBestAsk("ETHZ19");

                var btcBid = mdsSvc.GetBestBid("XBTUSD");
                var ethBid = mdsSvc.GetBestBid("ETHUSD");
                var xcfBid = mdsSvc.GetBestBid("ETHZ19");

                if (btcAsk > 0 && ethAsk > 0 && xcfAsk > 0 && btcBid > 0 && ethBid > 0 && xcfBid > 0)
                {
                    var btcposval = pSvc.GetQuantity("XBTUSD") / btcAsk;
                    var ethposval = pSvc.GetQuantity("ETHUSD") * ethBid * 0.000001;
                    var xcfposval = pSvc.GetQuantity("ETHZ19") * xcfAsk;

                    var grossposval = Math.Abs(btcposval) + Math.Abs(ethposval) + Math.Abs(xcfposval);

                    if (grossposval == 0)
                        grossposval = amount;

                    var currentQty = (long)Math.Round(xcfposval / xcfAsk, 0);
                    var targetposval = grossposval / 3.0;

                    if (mgsSvc.UnrealisedPnL >= 0 || counter > 9000 
                       || Math.Abs(btcposval) / Math.Abs(xcfposval) < 0.95 
                       || Math.Abs(ethposval) / Math.Abs(xcfposval) < 0.95
                       || margin < grossposval * 0.15)
                    {
                        if (margin < 0.15 * grossposval)
                        {
                            grossposval *= 0.8;
                            targetposval = grossposval / 3.0;
                        }

                        xcfmaker.FillTarget((long)Math.Round(-targetposval / xcfAsk, 0), true);

                        if (filledQty != 0 || currentQty != 0)
                        {
                            if (Math.Abs(filledQty) > Math.Abs(currentQty))
                                targetposval = filledQty * avgPx;
                            else
                                targetposval = xcfposval;

                            counter = 0;
                            
                            btcmaker.FillTarget((long)Math.Round(targetposval * btcBid, 0), true);
                            ethmaker.FillTarget((long)Math.Round((-targetposval / 0.000001) / ethAsk, 0), true);
                        }
                    }
                }

                Thread.Sleep(200);
            }

            bitmexSvc.Dispose();
        }
    }
}
