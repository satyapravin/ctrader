using Bitmex.Client.Websocket.Responses.Positions;
using EmbeddedService;
using System;
using System.Collections.Concurrent;

namespace PMS
{
    public class PositionService : IEmbeddedService
    {
        private ConcurrentDictionary<string, long> positions = new ConcurrentDictionary<string, long>();

        public void Start()
        {
            BitMex.BitMexService svc = (BitMex.BitMexService)Locator.Instance.GetService("BitMexService");
            svc.SubscribePositions(new BitMex.OnPosition(OnPositionUpdate));
        }

        private void OnPositionUpdate(PositionResponse response)
        {
            if (response.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Partial 
                || response.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Insert
                || response.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Update)
            {
                for(int ii=0; ii < response.Data.Length; ii++)
                {
                    var d = response.Data[ii];
                    long qty = 0;

                    if (d.CurrentQty.HasValue)
                    {
                        qty = d.CurrentQty.Value;
                    }

                    positions[d.Symbol] = qty;
                }
            }
            else if (response.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Delete)
            {
                for (int ii=0; ii < response.Data.Length; ii++)
                    positions[response.Data[ii].Symbol] = 0;
            }
        }

        public long GetQuantity(string symbol)
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
