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

        
        [Route("api/console/getsignature/{timeexpires}")]
        [HttpGet]
        public string GetSignature(int timeexpires)
        {
            return WebAPIStartup.GetSignature(timeexpires);
        }

        [HttpGet]
        [ActionName("getstrategysummary")]
        public IStrategySummary GetStrategySummary()
        {
            return WebAPIStartup.GetStrategySummary();
        }
    }
}
