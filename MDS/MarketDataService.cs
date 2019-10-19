using Bitmex.NET.Dtos;
using Bitmex.NET.Dtos.Socket;
using EmbeddedService;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ExchangeService;

namespace MDS
{
    public class MarketDataService : IEmbeddedService
    {
        private bool started = false;
        ConcurrentDictionary<string, OrderBook> booksBySymbol = new ConcurrentDictionary<string, OrderBook>();
        private List<string> symbols = new List<string>();

        public ServiceType Service { get { return ServiceType.MDS; } }
        public void Register(List<string> s)
        {
            symbols.AddRange(s);
        }
        
        public decimal GetBestBid(string symbol)
        {
            if (booksBySymbol.ContainsKey(symbol))
                return booksBySymbol[symbol].Bids.Expensive;
            else
                return -1;
        }

        public decimal GetBestAsk(string symbol)
        {
            if (booksBySymbol.ContainsKey(symbol))
                return booksBySymbol[symbol].Asks.Cheapest;
            else
                return -1;
        }

        public bool Start()
        {
            if (!this.started)
            { 
                var svc = (ExchangeService.Exchange)Locator.Instance.GetService(ServiceType.EXCHANGE);
                svc.Register(symbols);
                svc.SubscribeMarketData(new OnBookChanged(OnMarketData));
                this.started = true;
                return true;
            }
            else
            {
                throw new System.Exception("Starting twice");
            }
        }

        private void OnMarketData(BitmexSocketDataMessage<IEnumerable<OrderBookDto>> msg)
        {
            if (msg.Action == BitmexActions.Partial || msg.Action == BitmexActions.Insert)
            {
                if (msg.Action == BitmexActions.Partial)
                {
                    foreach (var book in msg.Data)
                        booksBySymbol.Remove(book.Symbol, out OrderBook destroy);
                }

                foreach (var d in msg.Data)
                {
                    if (!booksBySymbol.ContainsKey(d.Symbol))
                    {
                        booksBySymbol[d.Symbol] = new OrderBook();
                    }

                    var book = booksBySymbol[d.Symbol];

                    if (d.Price != 0 && d.Size != 0)
                    {
                        if (d.Side == "Buy")
                        {
                            book.Bids.Add(d.Id, d.Price, d.Size);
                        }
                        else if (d.Side == "Sell")
                        {
                            book.Asks.Add(d.Id, d.Price, d.Size);
                        }
                    }
                }
            }
            else if (msg.Action == BitmexActions.Delete)
            {
                foreach (var d in msg.Data)
                {
                    var book = booksBySymbol[d.Symbol];

                    if (d.Side == "Buy")
                    {

                        book.Bids.Delete(d.Id);
                    }
                    else if (d.Side == "Sell")
                    {
                        book.Asks.Delete(d.Id);
                    }                    
                }
            }
            else if (msg.Action == BitmexActions.Update)
            {
                foreach (var d in msg.Data)
                {
                    var book = booksBySymbol[d.Symbol];

                    if (d.Size != 0)
                    {
                        if (d.Side == "Buy")
                        {
                            book.Bids.Update(d.Id, d.Size);
                        }
                        else if (d.Side == "Sell")
                        {
                            book.Asks.Update(d.Id, d.Size);
                        }
                    }
                }
            }
        }

        public bool Stop()
        {
            return true;
        }
    }
}
