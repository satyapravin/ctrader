using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Utils;
using EmbeddedService;
using System;
using System.Threading;

namespace MGS
{
    public class MarginService : IEmbeddedService
    {
        private double availableMargin = 0;
        private double unrealizedPnl = 0;
        private double amount = 0;
        public void Start()
        {
            BitMex.BitMexService svc = (BitMex.BitMexService)Locator.Instance.GetService("BitMexService");
            svc.SubscribeMargin(new BitMex.OnMargin(OnMarginUpdate));
        }

        public double Amount { get { double retval = 0; Interlocked.Exchange(ref retval, amount); return retval; } }
        public double AvailableMargin { get { double retval = 0; Interlocked.Exchange(ref retval, availableMargin); return retval; } }
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
                    double avlblemgn = 0;
                    double upl = 0;
                    double amnt = 0;

                    if (d.AvailableMargin.HasValue)
                    {
                        double dval = d.AvailableMargin.Value;
                        avlblemgn = BitmexConverter.ConvertFromSatoshiToBtc(dval);
                    }

                    if (d.UnrealisedPnl.HasValue)
                    {
                        double dval = d.UnrealisedPnl.Value;
                        upl = BitmexConverter.ConvertFromSatoshiToBtc(dval);
                    }

                    if (d.Amount.HasValue)
                    {
                        double dval = d.Amount.Value;
                        amnt = BitmexConverter.ConvertFromSatoshiToBtc(dval);
                    }

                    Interlocked.Exchange(ref availableMargin, avlblemgn);
                    Interlocked.Exchange(ref unrealizedPnl, upl);
                    Interlocked.Exchange(ref amount, amnt);
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
