using EmbeddedService;
using Bitmex.NET.Dtos;
using Bitmex.NET.Dtos.Socket;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace PMS
{
    public class PositionService : IEmbeddedService
    {
        private ConcurrentDictionary<string, decimal> positions = new ConcurrentDictionary<string, decimal>();

        public ServiceType Service { get { return ServiceType.PMS; } }
        public bool Start()
        {
            Exchange.ExchangeService svc = (Exchange.ExchangeService)Locator.Instance.GetService(ServiceType.EXCHANGE);
            svc.SubscribePositions(new Exchange.OnPosition(OnPositionUpdate));
            return true;
        }

        private void OnPositionUpdate(BitmexSocketDataMessage<IEnumerable<PositionDto>> response)
        {
            if (response.Action == BitmexActions.Partial 
                || response.Action == BitmexActions.Insert
                || response.Action == BitmexActions.Update)
            {
                foreach(var d in response.Data)
                {
                    decimal qty = d.CurrentQty;
                    positions[d.Symbol] = qty;
                }
            }
            else if (response.Action == BitmexActions.Delete)
            {
                foreach(var d in response.Data)
                    positions[d.Symbol] = 0;
            }
        }

        public decimal GetQuantity(string symbol)
        {
            if (positions.ContainsKey(symbol))
            {
                return positions[symbol];
            }
            else 
            {
                return 0;
            }
        }

        public bool Stop()
        {
            return true;
        }
    }
}
