using CTrader.Interfaces;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CTrader.WebAPI
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ConsoleController : ApiController
    {
        [HttpGet]
        [ActionName("start")]
        public bool Start()
        {
            WebAPIStartup.Start();
            return true;
        }

        [HttpGet]
        [ActionName("stop")]
        public bool Stop()
        {
            WebAPIStartup.Stop();
            return true;
        }

        [HttpGet]
        [ActionName("rebalance")]
        public bool Rebalance()
        {
            WebAPIStartup.Rebalance();
            return true;
        }

        [HttpGet]
        [ActionName("getapikey")]
        public string GetAPIKey()
        {
            return WebAPIStartup.GetAPIKey();
        }

        [HttpGet]
        [ActionName("getapisecret")]
        public string GetAPISecret()
        {
            return WebAPIStartup.GetAPISecret();
        }

        [HttpGet]
        [ActionName("getsignature")]
        public string GetSignature(int timeExpires)
        {
            return WebAPIStartup.GetSignature(timeExpires);
        }

        [HttpGet]
        [ActionName("getstrategysummary")]
        public IStrategySummary GetStrategySummary()
        {
            return WebAPIStartup.GetStrategySummary();
        }
    }
}
