using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Utils;
using EmbeddedService;
using System;
using System.Threading;

namespace MGS
{
    public class MarginService : IEmbeddedService
    {
        private long availableMargin = 0;
        private double unrealizedPnl = 0;

        public void Start()
        {
            BitMex.BitMexService svc = (BitMex.BitMexService)Locator.Instance.GetService("BitMexService");
            svc.SubscribeMargin(new BitMex.OnMargin(OnMarginUpdate));
        }

        public long AvailableMargin {  get { return Interlocked.Read(ref availableMargin); } }
        public double UnrealisedPnL { get { double retval = 0;  Interlocked.Exchange(ref retval, unrealizedPnl); return retval; } }

        private void OnMarginUpdate(MarginResponse response)
        {
            if (response.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Partial
                || response.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Insert
                || response.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Update)
            {
                for (int ii = 0; ii < response.Data.Length; ii++)
                {
                    var d = response.Data[ii];
                    long avlblemgn = 0;
                    double upl = 0;
                    
                    if (d.AvailableMargin.HasValue)
                    {
                        avlblemgn = d.AvailableMargin.Value;
                    }

                    if (d.UnrealisedPnl.HasValue)
                    {
                        double dval = d.UnrealisedPnl.Value;
                        upl = BitmexConverter.ConvertFromSatoshiToBtc(dval);
                    }


                    Interlocked.Exchange(ref availableMargin, avlblemgn);
                    Interlocked.Exchange(ref unrealizedPnl, upl);
                }
            }
            else if (response.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Delete)
            {
                for (int ii = 0; ii < response.Data.Length; ii++)
                {
                    Interlocked.Exchange(ref availableMargin, 0);
                    Interlocked.Exchange(ref unrealizedPnl, 0);
                }
            }
        }

        public bool Stop()
        {
            return true;
        }
    }
}
