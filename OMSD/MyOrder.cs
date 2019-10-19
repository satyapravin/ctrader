using Bitmex.NET.Dtos;
using OMS.Logging;

namespace OMS
{
    public enum RequestType { NEW, CANCEL, AMEND };
    public enum OrderStateIdentifier { UNCONFIRMED, CONFIRMED }

    public class MyOrder
    {
        private static ILog Log = LogProvider.GetCurrentClassLogger(); 
        public enum OrderSide { UNDEFINED, BUY, SELL };

        public enum OrderType { UNDEFINED, MARKET, LIMIT, LIMIT_POST };
        
        private OrderMgmtService oms_server = null;
        public MyOrder(OrderMgmtService svc)
        {
            oms_server = svc;
        }

        public void Send()
        {
            if (this.Request == RequestType.NEW && Status == OrderStateIdentifier.UNCONFIRMED)
            {
                oms_server.NewOrder(this);
            }
            else
            {
                Log.Error($"Wrong new on order {Symbol}, {this.Request}, {this.Status}");
            }
        }

        public void Cancel()
        {
            if (Status == OrderStateIdentifier.CONFIRMED && this.Request != RequestType.CANCEL)
            {
                oms_server.CancelOrder(this);
            }
            else
            {
                Log.Error($"Wrong cancel on order {Symbol}, {this.Request}, {this.Status}");
            }
        }

        public void Amend(decimal price)
        {
            if (Status == OrderStateIdentifier.CONFIRMED)
            {
                oms_server.AmendOrder(this, price);
            }
            else
            {
                Log.Error($"Wrong amend on order {Symbol}, {this.Request}, {this.Status}");
            }
        }
 

        public RequestType Request { get; set; }
        public MyOrder ChildOrder { get; set; }
        public OrderDto Execution { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public OrderStateIdentifier Status { get; set; }
        public string Symbol { get; set; }

        public OrderSide Side { get; set; }
        public string ClientOrderID { get; set; }
        public OrderType Type { get; set; }        
    }
}
