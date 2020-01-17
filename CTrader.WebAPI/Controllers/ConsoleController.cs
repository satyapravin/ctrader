using System.Web.Http;

namespace CTrader.WebAPI
{
    public class ConsoleController : ApiController
    {
        [HttpGet]
        [ActionName("start")]
        public bool Start()
        {
            return true;
        }

        [HttpGet]
        [ActionName("stop")]
        public bool Stop()
        {
            return true;
        }

        [HttpGet]
        [ActionName("rebalance")]        
        public bool Rebalance()
        {
            return true;
        }
    }
}
