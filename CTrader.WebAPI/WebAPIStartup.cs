using Bitmex.NET.Authorization;
using CTrader.Interfaces;
using System.Configuration;

namespace CTrader.WebAPI
{
    public class WebAPIStartup
    {
        static IStrategy _strategy;
        public static void Start(IStrategy strategy)
        {
            _strategy = strategy;
        }
        public static void Start()
        {
            _strategy.Start();
        }
        public static void Stop()
        {
            _strategy.Stop();
        }
        public static void Rebalance()
        {
            _strategy.Rebalance();
        }
        public static string GetAPIKey()
        {
            return ConfigurationManager.AppSettings["APIKEY"];
        }
        public static string GetAPISecret()
        {
            return ConfigurationManager.AppSettings["APISECRET"];
        }
        public static string GetSignature(int expiresTime)
        {
            return (new SignatureProvider()).CreateSignature(WebAPIStartup.GetAPISecret(), $"GET/realtime{expiresTime}");
        }
        public static IStrategySummary GetStrategySummary()
        {
            return _strategy.GetSummary();
        }
    }
}
