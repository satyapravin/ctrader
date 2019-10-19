using Bitmex.NET.Dtos;

namespace OMS
{
    public enum RequestType { NEW, CANCEL, AMEND };
    public enum OrderStateIdentifier { UNCONFIRMED, CONFIRMED }

    public class MyOrder
    {
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
                System.Console.WriteLine("Sending wrong order " + this.Request.ToString() + " " + Status.ToString());
            }
        }

        public void Cancel()
        {
            if (this.Request == RequestType.CANCEL && Status == OrderStateIdentifier.UNCONFIRMED)
            {
                oms_server.CancelOrder(this);
            }
            else
            {
                System.Console.WriteLine("Sending wrong order " + this.Request.ToString() + " " + Status.ToString());
            }
        }

        public void Amend(decimal price)
        {
            if (this.Request == RequestType.AMEND && Status == OrderStateIdentifier.UNCONFIRMED)
            {
                oms_server.AmendOrder(this, price);
            }
            else
            {
                System.Console.WriteLine("Sending wrong order " + this.Request.ToString() + " " + Status.ToString());
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
