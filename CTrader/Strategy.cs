using Bitmex.NET.Dtos;
using Bitmex.NET.Models;
using CTrader.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static CTrader.SlackClient;

namespace CTrader
{
    public class Strategy : IStrategy
    {
        log4net.ILog log = log4net.LogManager.GetLogger(typeof(Strategy));
        private enum Command {  START, STOP, REBALANCE };
        private StrategySummary strategySummary = new StrategySummary();
        private BlockingCollection<Command> queue = new BlockingCollection<Command>();
        private System.Timers.Timer timer = new System.Timers.Timer();
        private decimal leverage = 20.0m;
        private string environment = "TEST";
        private XBTUSD xbtProperties = new XBTUSD();
        private ETHUSD ethProperties = new ETHUSD();
        private XCFUSD xcfProperties = new XCFUSD();
        private decimal profit = 0;
        private decimal previousNotional = 0;

        private int counter = 180;
        private ExchangeService.Exchange exchange = null;
        private SlackClient slackMessenger = null;

        public Strategy(string apiKey, string apiSecret, bool isLive, string slackUrl)
        {
            if (isLive)
                environment = "PROD";

            slackMessenger = new SlackClient(slackUrl);
            exchange = new ExchangeService.Exchange(apiKey, apiSecret, isLive);
            HashSet<string> symbols = new HashSet<string>
            {
                xbtProperties.Reference(),
                ethProperties.Reference()
            };

            exchange.marketDataSystem.RegisterInstruments(symbols);
            symbols.Clear();
            symbols.Add(xbtProperties.Symbol());
            symbols.Add(xcfProperties.Symbol());
            symbols.Add(ethProperties.Symbol());
            exchange.marketDataSystem.Register(symbols);
            exchange.Start().WaitOne();

            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Interval = 20000;
            Thread t = new Thread(OnCreate);
            t.Start();
            log.Info("Strategy created");
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ++counter;
            RebalancePortfolio();
        }

        public void OnCreate()
        {
            log.Info("Strategy thread created");
            while(true)
            {
                var cmd = queue.Take();

                if (cmd == Command.START)
                {
                    if (!timer.Enabled)
                    {
                        timer.Enabled = true;
                        strategySummary.State = "Running";
                        log.Info("Strategy started");
                    }
                    else
                    {
                        log.Info("Strategy already running");
                    }
                }
                else if (cmd == Command.STOP)
                {
                    if (timer.Enabled)
                    {
                        timer.Enabled = false;
                        strategySummary.State = "Stopped";
                        log.Info("Strategy stopped");
                    }
                    else
                    {
                        log.Info("Cannot stop a strategy when not running");
                    }
                }
                else if (cmd == Command.REBALANCE)
                {
                    log.Info("Strategy received manual rebalance");
                    RebalancePortfolio();
                    log.Info("Strategy rebalanced");
                }
            }
        }

        private void RebalancePortfolio()
        {
            StrategySummary summary = new StrategySummary();
            summary.Leverage = leverage;
            summary.Environment = environment;

            try
            {
                var marginParams = new UserMarginGETRequestParams
                {
                    Currency = "XBt"
                };

                var marginTask = exchange.GetMargin(marginParams);
                var margin = marginTask.Result.Result;
                summary.BalanceMargin = margin.MarginBalance.HasValue ? margin.MarginBalance.Value : 0;
                summary.RealizedPnl = margin.RealisedPnl.HasValue ? margin.RealisedPnl.Value : 0;
                summary.UnrealizedPnl = margin.UnrealisedPnl.HasValue ? margin.UnrealisedPnl.Value : 0;
                summary.WalletBalance = margin.WalletBalance.HasValue ? margin.WalletBalance.Value : 0;
                summary.UsedMargin = margin.MarginUsedPcnt.HasValue ? (decimal)margin.MarginUsedPcnt.Value : 0;
                var totalBalance = ConvertFromSatoshiToBtc(margin.MarginBalance.Value);
                var notional = totalBalance / 3.0m * leverage;

                if (previousNotional == 0)
                    previousNotional = notional;

                var positionTask = exchange.GetPositions(new PositionGETRequestParams());
                var positions = positionTask.Result.Result;

                decimal xbtQty = 0;
                decimal ethQty = 0;
                decimal xcfQty = 0;

                decimal xbtEntry = 0;
                decimal ethEntry = 0;
                decimal xcfEntry = 0;

                foreach (var p in positions)
                {
                    if (p.Symbol == xbtProperties.Symbol())
                    {
                        xbtQty = p.CurrentQty;
                        xbtEntry = p.AvgEntryPrice.HasValue ? p.AvgEntryPrice.Value : 0;
                    }
                    else if (p.Symbol == ethProperties.Symbol())
                    {
                        ethQty = p.CurrentQty;
                        ethEntry = p.AvgEntryPrice.HasValue ? p.AvgEntryPrice.Value : 0;
                    }
                    else if (p.Symbol == xcfProperties.Symbol())
                    {
                        xcfQty = p.CurrentQty;
                        xcfEntry = p.AvgEntryPrice.HasValue ? p.AvgEntryPrice.Value : 0;
                    }
                }

                decimal xbtBid = exchange.marketDataSystem.GetBestBid(xbtProperties.Symbol());
                decimal xbtAsk = exchange.marketDataSystem.GetBestAsk(xbtProperties.Symbol());
                decimal ethBid = exchange.marketDataSystem.GetBestBid(ethProperties.Symbol());
                decimal ethAsk = exchange.marketDataSystem.GetBestAsk(ethProperties.Symbol());

                if (xbtBid > 0 && xbtAsk > 0 && ethBid > 0 && ethAsk > 0)
                {
                    if (Math.Abs(notional - previousNotional) / notional > 10.5m)
                    {
                        previousNotional = notional;
                    }
                    else
                    {
                        notional = previousNotional;
                    }

                    log.Info(string.Format("Trading notional {0:N4}", notional));
                    decimal runningPnl = 0;

                    log.Info(string.Format("XBT current {0}, XBT last {1}, ETH current {2}, ETH last {3} XCF current {4:N5}, XCF last {5:N5}", 
                                            xbtAsk, xbtEntry, ethAsk, ethEntry, ethAsk / xbtAsk, ethEntry / xbtEntry));
                    decimal xbtPnl = xbtProperties.GetPnl(xbtQty, xbtAsk, xbtEntry);
                    decimal ethPnl = ethProperties.GetPnl(ethQty, ethBid, ethEntry);
                    decimal xcfPnl = xcfProperties.GetPnl(xcfQty, ethBid / xbtAsk, ethEntry / xbtEntry);
                    runningPnl = xbtPnl + ethPnl + xcfPnl;
                    
                        
                    log.Info(string.Format("{0:N5} XBT Pnl {1:N5} ETH Pnl {2:N5} XCF Pnl {3:N5} runningPnl (Xbt)", 
                                xbtPnl, ethPnl, xcfPnl, runningPnl * 100000000));

                    runningPnl -= profit;
                    
                    var xcfTotalQty = Math.Round(xcfProperties.GetQuantity(-notional, ethAsk / xbtBid, ethBid / xbtAsk));
                    var tradenotional = Math.Abs(xcfProperties.GetPositionValue(xcfTotalQty, ethAsk / xbtBid, ethBid / xbtAsk));
                    var xbtTotalQty = xbtProperties.GetQuantity(-tradenotional, xbtBid, xbtAsk);
                    tradenotional = Math.Abs(xbtProperties.GetPositionValue(xbtTotalQty, xbtBid, xbtAsk));
                    var ethTotalQty = ethProperties.GetQuantity(tradenotional, ethBid, ethAsk);
                    var ethToTrade = Math.Round(ethTotalQty - ethQty);
                    var xcfToTrade = Math.Round(xcfTotalQty - xcfQty);
                    var xbtToTrade = Math.Round(xbtTotalQty - xbtQty);

                    decimal commission = Math.Abs(ethProperties.GetPositionValue(ethToTrade, ethBid, ethAsk)) 
                                        + Math.Abs(xbtProperties.GetPositionValue(xbtToTrade, xbtBid, xbtAsk));
                    commission *= 0.00075m;
                    commission += Math.Abs(xcfProperties.GetPositionValue(xcfToTrade, ethAsk / xbtBid, ethBid / xbtAsk)) * 0.0025m;

                    log.Info(string.Format("Commission {0:N5} Xbt", commission * 100000000));
                    if (runningPnl < 2 * commission && counter < 180 && xcfToTrade == 0)
                    {
                        return;
                    }

                    profit += runningPnl;

                    if (xbtToTrade == 0 && ethToTrade == 0 && xcfToTrade == 0)
                    {
                        log.Info("Nothing to trade - all trades are zero");
                        return;
                    }

                    OrderPOSTRequestParams xbtOrder = new OrderPOSTRequestParams
                    {
                        Symbol = xbtProperties.Symbol()
                    };
                    if (xbtToTrade < 0)
                        xbtOrder.Side = "Sell";
                    else
                        xbtOrder.Side = "Buy";
                    xbtOrder.OrderQty = Math.Abs(xbtToTrade);
                    xbtOrder.OrdType = "Market";

                    OrderPOSTRequestParams ethOrder = new OrderPOSTRequestParams
                    {
                        Symbol = ethProperties.Symbol()
                    };
                    if (ethToTrade < 0)
                        ethOrder.Side = "Sell";
                    else
                        ethOrder.Side = "Buy";
                    ethOrder.OrderQty = Math.Abs(ethToTrade);
                    ethOrder.OrdType = "Market";

                    OrderPOSTRequestParams xcfOrder = new OrderPOSTRequestParams
                    {
                        Symbol = xcfProperties.Symbol()
                    };
                    if (xcfToTrade < 0)
                        xcfOrder.Side = "Sell";
                    else
                        xcfOrder.Side = "Buy";
                    xcfOrder.OrderQty = Math.Abs(xcfToTrade);
                    xcfOrder.OrdType = "Market";
                    OrderDto result;

                    if (xbtToTrade != 0)
                    {
                        result = exchange.PostOrder(xbtOrder).Result.Result;
                        log.Info(String.Format("Traded {0} of {1}", xbtToTrade, result.Symbol));
                    }
                    else
                    {
                        log.Info("No need to rebalance XBT");
                    }

                    if (xcfToTrade != 0)
                    {
                        result = exchange.PostOrder(xcfOrder).Result.Result;
                        log.Info(String.Format("Traded {0} of {1}", xcfToTrade, result.Symbol));
                    }
                    else
                    {
                        log.Info("No need to rebalance XCF");
                    }

                    if (ethToTrade != 0)
                    {
                        result = exchange.PostOrder(ethOrder).Result.Result;
                        log.Info(String.Format("Traded {0} of {1}", ethToTrade, result.Symbol));
                    }
                    else
                    {
                        log.Info("No need to rebalance ETH");
                    }
                    summary.State = "Running";
                    counter = 0;
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                summary.State = "Exception";
            }
            finally
            {
                Interlocked.Exchange<StrategySummary>(ref strategySummary, summary);
            }

            try
            {
                SlackMessage msg = new SlackMessage();
                msg.Text = string.Format("{0}:  WalletBalance={1:N6} BTC     BalanceMargin={2:N6} BTC     profit={3:N6} BTC", 
                    summary.Environment, summary.WalletBalance * 0.00000001m, summary.BalanceMargin * 0.00000001m, profit);
                slackMessenger.SendSlackMessage(msg);

            }
            catch(Exception e)
            {
                log.Error(e);
            }
        }

        public void Start()
        {
            queue.Add(Command.START);
        }

        public void Stop()
        {
            queue.Add(Command.STOP);
        }

        public void Rebalance()
        {
            queue.Add(Command.REBALANCE);
        }

        public IStrategySummary GetSummary()
        {
            StrategySummary retval = null;
            Interlocked.Exchange<StrategySummary>(ref retval, strategySummary);
            return retval;
        }

        public static decimal ConvertFromSatoshiToBtc(decimal value)
        {
            return value * 0.00000001m;
        }
    }
}
