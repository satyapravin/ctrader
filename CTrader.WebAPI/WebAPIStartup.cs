using Bitmex.NET.Authorization;
using CTrader.Interfaces;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;
using log4net;
using System.Configuration;

namespace CTrader.WebAPI
{
    public class WebAPIStartup
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger("Startup");
        static IStrategy _strategy;
        public static void Start(IStrategy strategy)
        {
            _strategy = strategy;

            var config = new HttpSelfHostConfiguration("http://localhost:7080");
            config.Routes.MapHttpRoute("API Default", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            config.EnableCors();

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Log.Debug($"Started CTrader WebAPI");
                Thread.Sleep(Timeout.Infinite);
            }
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
