using Bitmex.NET.Dtos;
using Bitmex.NET.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTrader
{
    class Program
    {
        public static decimal ConvertFromSatoshiToBtc(decimal value)
        {
            return value * 0.00000001m;
        }

        static void Main(string[] args)
        {
            XBTUSD xbtProperties = new XBTUSD();
            ETHUSD ethProperties = new ETHUSD();
            XCFUSD xcfProperties = new XCFUSD();

            string apiKey = ConfigurationManager.AppSettings["APIKEY"];
            string apiSecret = ConfigurationManager.AppSettings["APISECRET"];
            bool isLive = bool.Parse(ConfigurationManager.AppSettings["ISLIVE"]);
            ExchangeService.Exchange exch = new ExchangeService.Exchange(apiKey, apiSecret, isLive);

            HashSet<string> symbols = new HashSet<string>
            {
                xbtProperties.Reference(),
                xcfProperties.Reference()
            };
            exch.marketDataSystem.RegisterInstruments(symbols);
            exch.Start().WaitOne();


            while (true)
            {
                try
                {
                    var marginParams = new UserMarginGETRequestParams
                    {
                        Currency = "XBt"
                    };
                    var marginTask = exch.GetMargin(marginParams);
                    var margin = marginTask.Result.Result;
                    var totalBalance = ConvertFromSatoshiToBtc(margin.MarginBalance.Value);
                    var notional = totalBalance / 3.0m * 20.0m;
                    var positionTask = exch.GetPositions(new PositionGETRequestParams());
                    var positions = positionTask.Result.Result;

                    decimal xbtQty = 0;
                    decimal ethQty = 0;
                    decimal xcfQty = 0;

                    foreach(var p in positions)
                    {
                        if (p.Symbol == xbtProperties.Symbol())
                        {
                            xbtQty = p.CurrentQty;
                        }
                        else if (p.Symbol == ethProperties.Symbol())
                        {
                            ethQty = p.CurrentQty;
                        }
                        else if (p.Symbol == xcfProperties.Symbol())
                        {
                            xcfQty = p.CurrentQty;
                        }
                    }

                    decimal xbtPrice = exch.marketDataSystem.GetInstrLast(xbtProperties.Reference());
                    decimal xcfPrice = exch.marketDataSystem.GetInstrLast(xcfProperties.Reference());
                    decimal ethPrice = xcfPrice * xbtPrice;

                    if (xbtPrice > 0 || ethPrice > 0)
                    {
                        var xcfTotalQty = Math.Round(xcfProperties.GetQuantity(-notional, xcfPrice, xcfPrice));
                        notional = Math.Abs(xcfProperties.GetPositionValue(xcfTotalQty, xcfPrice, xcfPrice));
                        var xbtTotalQty = xbtProperties.GetQuantity(-notional, xbtPrice, xbtPrice);
                        var ethTotalQty = ethProperties.GetQuantity(notional, ethPrice, ethPrice);
                        var ethToTrade = Math.Round(ethTotalQty - ethQty);
                        var xcfToTrade = Math.Round(xcfTotalQty - xcfQty);
                        var xbtToTrade = Math.Round(xbtTotalQty - xbtQty);

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
                            result = exch.PostOrder(xbtOrder).Result.Result;
                            System.Console.WriteLine(result.Symbol);
                        }

                        if (xcfToTrade != 0)
                        {
                            result = exch.PostOrder(xcfOrder).Result.Result;
                            System.Console.WriteLine(result.Symbol);
                        }

                        if (ethToTrade != 0)
                        {
                            result = exch.PostOrder(ethOrder).Result.Result;
                            System.Console.WriteLine(result.Symbol);
                        }
                        Thread.Sleep(3600 * 1000);
                    }

                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
