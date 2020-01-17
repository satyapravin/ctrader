using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;
using log4net;
namespace CTrader.WebAPI
{
    public class Startup
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger("Startup");
        public static void Start()
        {
            var config = new HttpSelfHostConfiguration("http://localhost:7080");
            config.Routes.MapHttpRoute("API Default", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Log.Debug($"Started CTrader WebAPI");
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}