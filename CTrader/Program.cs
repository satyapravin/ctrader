using EmbeddedService;
using System;
using System.Collections.Generic;
using ExchangeService;
using System.Reflection;
using System.IO;
using System.Threading;
using MDS;
using PMS;
using MGS;
using OMS;
using log4net;
using log4net.Config;
using Bitmex.NET.Dtos;
using CTrader.Logging;
using ILog = CTrader.Logging.ILog;

namespace CTrader
{
    public class Program
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private static void InitLogging()
        {
            XmlConfigurator.Configure(
                LogManager.GetRepository(Assembly.GetAssembly(typeof(LogManager))),
                new FileInfo("log4net.config"));
        }

        public static void processOMSMessage(OrderDto o)
        {
            if (o.OrdStatus == "Filled" || o.OrdStatus == "PartiallyFilled")
            {
                lock (lObject)
                {
                    avgPx = o.AvgPx.HasValue ? o.AvgPx.Value : 0;
                    filledQty = o.OrderQty.HasValue ? o.OrderQty.Value : 0;

                    if (o.Side == "Sell")
                    {
                        filledQty *= -1;
                    }
                }
            }
        }

        public static object lObject = new object();
        public static decimal avgPx = 0;
        public static decimal filledQty = 0;

        public static void Main()
        {
            InitLogging();
            Log.Info("Starting CTrader");            
            Exchange bitmexSvc = new Exchange(System.Configuration.ConfigurationManager.AppSettings["ApiKey"],
                                              System.Configuration.ConfigurationManager.AppSettings["ApiSecret"],
                                              bool.Parse(System.Configuration.ConfigurationManager.AppSettings["IsLive"]));

            MarketDataService mdsSvc = new MarketDataService();
            PositionService pSvc = new PositionService();
            OrderMgmtService omsSvc =  new OrderMgmtService();
            MarginService mgsSvc = new MarginService();

            Locator.Instance.Register(bitmexSvc);
            Locator.Instance.Register(mdsSvc);
            Locator.Instance.Register(pSvc);
            Locator.Instance.Register(omsSvc);
            Locator.Instance.Register(mgsSvc);

            Executor.XBTUSD InstrXBTUSD = new Executor.XBTUSD();
            Executor.ETHUSD InstrETHUSD = new Executor.ETHUSD();
            Executor.XCFUSD InstrXCFUSD = new Executor.XCFUSD();

            List<string> symbols = new List<string>();
            symbols.Add(InstrXBTUSD.Symbol());
            symbols.Add(InstrETHUSD.Symbol());
            symbols.Add(InstrXCFUSD.Symbol());

            omsSvc.RegisterHandler(InstrXCFUSD.Symbol(), new OMS.OnOrder(processOMSMessage));
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
                var amount = mgsSvc.Amount * 0.75m;
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
                    decimal targetposval = 0;

                    if (grossposval == 0)
                        targetposval = amount / 3.0m;
                    else if (xcfposval != 0)
                        targetposval = Math.Abs(xcfposval) / 3.0m;

                    xcfmaker.FillTarget(-targetposval, xcfAsk, true);
                    var currentQty = pSvc.GetQuantity(InstrXCFUSD.Symbol());

                    decimal fQty = 0;

                    lock(lObject) { fQty = filledQty; }

                    decimal fillposval = 0;

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
        }
    }
}
