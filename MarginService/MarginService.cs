using Bitmex.NET.Dtos;
using Bitmex.NET.Dtos.Socket;
using EmbeddedService;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MGS
{
    public class MarginService : IEmbeddedService
    {
        private decimal availableMargin = 0;
        private decimal unrealizedPnl = 0;
        private decimal amount = 0;

        public ServiceType Service { get { return ServiceType.MGS; } }

        public bool Start()
        {
            ExchangeService.Exchange svc = (ExchangeService.Exchange)Locator.Instance.GetService(ServiceType.EXCHANGE);
            svc.SubscribeMargin(new ExchangeService.OnMargin(new ExchangeService.OnMargin(OnMarginUpdate)));
            return true;
        }

        public decimal Amount
        {
            get { lock (this) { return this.amount; } }
        }        
        public decimal AvailableMargin
        {
            get { lock (this) { return this.availableMargin; } }
        }
        public decimal UnrealisedPnL
        {
            get { lock (this) { return this.unrealizedPnl; } }
        }

        private void OnMarginUpdate(BitmexSocketDataMessage<IEnumerable<MarginDto>> response)
        {
            if (response.Action == BitmexActions.Partial
                || response.Action == BitmexActions.Insert
                || response.Action == BitmexActions.Update)
            {
                foreach (var d in response.Data)
                {
                    decimal avlblemgn = 0;
                    decimal upl = 0;
                    decimal amnt = 0;

                    if (d.AvailableMargin.HasValue)
                    {
                        decimal dval = d.AvailableMargin.Value;
                        avlblemgn = ConvertFromSatoshiToBtc(dval);
                    }

                    if (d.UnrealisedPnl.HasValue)
                    {
                        decimal dval = d.UnrealisedPnl.Value;
                        upl = ConvertFromSatoshiToBtc(dval);
                    }

                    if (d.Amount.HasValue)
                    {
                        decimal dval = d.Amount.Value;
                        amnt = ConvertFromSatoshiToBtc(dval);
                    }

                    lock(this)
                    {
                        amount = amnt;
                        availableMargin = avlblemgn;
                        unrealizedPnl = upl;
                    }

                }
            }
            else if (response.Action == BitmexActions.Delete)
            {
                foreach (var d in response.Data)
                {
                    lock(this)
                    {
                        availableMargin = 0;
                        amount = 0;
                        unrealizedPnl = 0;
                    }
                }
            }
        }

        public static decimal ConvertFromSatoshiToBtc(decimal value)
        {
            return value * 0.00000001m;
        }
        public bool Stop()
        {
            return true;
        }
    }
}
