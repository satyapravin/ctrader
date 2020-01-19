using System.Web.Http;

namespace CTrader.WebAPI
{
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
        public bool GetAPIKey()
        {
            WebAPIStartup.GetAPIKey();
            return true;
        }

        [HttpGet]
        [ActionName("getapisecret")]
        public bool GetAPISecret()
        {
            WebAPIStartup.GetAPISecret();
            return true;
        }

        [HttpGet]
        [ActionName("getsignature")]
        public string GetSignature(int timeExpires)
        {
            return WebAPIStartup.GetSignature(timeExpires);
        }
    }
}
