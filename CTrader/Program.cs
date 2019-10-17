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
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Verbose)
                .WriteTo.Console(LogEventLevel.Verbose)
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
            lock (lObject)
            {
                avgPx = ntfy.avgPx;

                filledQty = ntfy.Qty;

                if (!req.isBuy && filledQty > 0)
                    filledQty *= -1;
            }
        }

        public static void OnOrderPartialFilled(OMS.OrderPartiallyFilled ntfy, OrderRequest req)
        {
            lock (lObject)
            {
                avgPx = ntfy.avgPx;
                filledQty = ntfy.cumQty;

                if (!req.isBuy && filledQty > 0)
                    filledQty *= -1;
            }
        }

        public static void OnOrderRejected(OMS.OrderRejected ntfy, OrderRequest req)
        {

        }

        public static object lObject = new object();
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

            Executor.XBTUSD InstrXBTUSD = new Executor.XBTUSD();
            Executor.ETHUSD InstrETHUSD = new Executor.ETHUSD();
            Executor.XCFUSD InstrXCFUSD = new Executor.XCFUSD();

            List<string> symbols = new List<string>();
            symbols.Add(InstrXBTUSD.Symbol());
            symbols.Add(InstrETHUSD.Symbol());
            symbols.Add(InstrXCFUSD.Symbol());

            omsSvc.RegisterHandler(InstrXCFUSD.Symbol(), new Tuple<OnOrderAck, 
                                                                   OnOrderCanceled, 
                                                                   OnOrderFilled, 
                                                                   OnOrderPartiallyFilled, 
                                                                   OnOrderRejected>(OnOrderAck, OnOrderCanceld, OnOrderFilled,
                                                                                    OnOrderPartialFilled, OnOrderRejected));
            mdsSvc.Register(symbols);
            mdsSvc.Start();
            pSvc.Start();
            mgsSvc.Start();
            omsSvc.Start();
            bitmexSvc.Start();
            Executor.MarketMaker xcfmaker = new Executor.MarketMaker(InstrXCFUSD);
            Executor.MarketMaker btcmaker = new Executor.MarketMaker(InstrXBTUSD);
            Executor.MarketMaker ethmaker = new Executor.MarketMaker(InstrETHUSD);

            ethmaker.Start();
            btcmaker.Start();
            xcfmaker.Start();


            while (true)
            {
                var amount = mgsSvc.Amount * 0.75;
                amount *= 30;

                var btcAsk = mdsSvc.GetBestAsk(InstrXBTUSD.Symbol());
                var ethAsk = mdsSvc.GetBestAsk(InstrETHUSD.Symbol());
                var xcfAsk = mdsSvc.GetBestAsk(InstrXCFUSD.Symbol());

                var btcBid = mdsSvc.GetBestBid(InstrXBTUSD.Symbol());
                var ethBid = mdsSvc.GetBestBid(InstrETHUSD.Symbol());
                var xcfBid = mdsSvc.GetBestBid(InstrXCFUSD.Symbol());

                if (btcAsk > 0 && ethAsk > 0 && xcfAsk > 0 && btcBid > 0 && ethBid > 0 && xcfBid > 0)
                {
                    var btcposval = InstrXBTUSD.GetPositionValue(pSvc.GetQuantity(InstrXBTUSD.Symbol()), btcBid, btcAsk);
                    var ethposval = InstrETHUSD.GetPositionValue(pSvc.GetQuantity(InstrETHUSD.Symbol()), ethBid, ethAsk);
                    var xcfposval = InstrXCFUSD.GetPositionValue(pSvc.GetQuantity(InstrXCFUSD.Symbol()), xcfBid, xcfAsk);
                    
                    var grossposval = Math.Abs(btcposval) + Math.Abs(ethposval) + Math.Abs(xcfposval);
                    double targetposval = 0;

                    if (grossposval == 0)
                        targetposval = amount / 3.0;
                    else if (xcfposval != 0)
                        targetposval = Math.Abs(xcfposval) / 3.0;

                    xcfmaker.FillTarget(-targetposval, xcfAsk, true);
                    var currentQty = pSvc.GetQuantity(InstrXCFUSD.Symbol());

                    long fQty = 0;

                    lock(lObject) { fQty = filledQty; }

                    double fillposval = 0;

                    if (fQty != 0 || currentQty != 0)
                    {
                        if (Math.Abs(fQty) > Math.Abs(currentQty))
                            fillposval = fQty * avgPx;
                        else
                            fillposval = currentQty * (currentQty > 0 ? xcfBid : xcfAsk);
                    }

                    btcmaker.FillTarget(fillposval, fillposval > 0 ? btcBid : btcAsk, true);
                    ethmaker.FillTarget(-fillposval, -fillposval > 0 ? ethBid : ethAsk, true);
                }

                Thread.Sleep(2000);
            }

            bitmexSvc.Dispose();
        }
    }
}
