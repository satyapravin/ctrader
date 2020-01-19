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
        private static Strategy strategy = null;
        static string ApiKey { get; set; }
        static string ApiSecret { get; set; }
        static string IsLive { get; set; }

        static StrategySummary GetSummary()
        {
            return strategy != null ? strategy.GetSummary() : null;
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            string apiKey = ConfigurationManager.AppSettings["APIKEY"];
            string apiSecret = ConfigurationManager.AppSettings["APISECRET"];
            bool isLive = bool.Parse(ConfigurationManager.AppSettings["ISLIVE"]);
            Strategy strategy = new Strategy(apiKey, apiSecret, isLive);
            strategy.Start();
            Thread.Sleep(0);
        }
    }
}
