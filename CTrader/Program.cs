using CTrader.Interfaces;
using CTrader.WebAPI;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace CTrader
{
    class Program
    { 
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            string apiKey = ConfigurationManager.AppSettings["APIKEY"];
            string apiSecret = ConfigurationManager.AppSettings["APISECRET"];
            bool isLive = bool.Parse(ConfigurationManager.AppSettings["ISLIVE"]);
            string slackUrl = ConfigurationManager.AppSettings["SLACKURL"];
            IStrategy strategy = new Strategy(apiKey, apiSecret, isLive, slackUrl);
            strategy.Start();
            Task.Run(() => WebAPIStartup.Start(strategy));
            Thread.Sleep(0);
        }
    }
}
