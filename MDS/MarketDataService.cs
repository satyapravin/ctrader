using EmbeddedService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BitMex;
using Bitmex.Client.Websocket.Responses.Books;

namespace MDS
{
    public class MarketDataService : IEmbeddedService
    {
        ConcurrentDictionary<string, OrderBook> booksBySymbol = new ConcurrentDictionary<string, OrderBook>();
        private List<string> symbols = new List<string>();

        public void Register(List<string> s)
        {
            symbols.AddRange(s);
        }
        
        public double GetBestBid(string symbol)
        {
            if (booksBySymbol.ContainsKey(symbol))
                return booksBySymbol[symbol].Bids.Expensive;
            else
                return -1;
        }

        public double GetBestAsk(string symbol)
        {
            if (booksBySymbol.ContainsKey(symbol))
                return booksBySymbol[symbol].Asks.Cheapest;
            else
                return -1;
        }

        public void Start()
        {
            var svc = (BitMexService)Locator.Instance.GetService("BitMexService");
            svc.Register(symbols);
            svc.SubscribeMarketData(new OnBookChanged(OnMarketData));
        }

        private void OnMarketData(BookResponse msg)
        {
            if (msg.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Partial || msg.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Insert)
            {
                if (msg.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Partial)
                {
                    booksBySymbol.Remove(msg.Data[0].Symbol, out OrderBook destroy);
                }

                for(int ii=0; ii < msg.Data.Length; ii++)
                {
                    var d = msg.Data[ii];

                    if (!booksBySymbol.ContainsKey(d.Symbol))
                    {
                        booksBySymbol[d.Symbol] = new OrderBook();
                    }

                    var book = booksBySymbol[d.Symbol];

                    if (d.Price.HasValue && d.Size.HasValue)
                    {
                        if (d.Side == Bitmex.Client.Websocket.Responses.BitmexSide.Buy)
                        {

                            book.Bids.Add(d.Id, d.Price.Value, d.Size.Value);
                        }
                        else if (d.Side == Bitmex.Client.Websocket.Responses.BitmexSide.Sell)
                        {
                            book.Asks.Add(d.Id, d.Price.Value, d.Size.Value);
                        }
                    }
                }
            }
            else if (msg.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Delete)
            {
                for (int ii = 0; ii < msg.Data.Length; ii++)
                {
                    var d = msg.Data[ii];

                    var book = booksBySymbol[d.Symbol];

                    if (d.Side == Bitmex.Client.Websocket.Responses.BitmexSide.Buy)
                    {

                        book.Bids.Delete(d.Id);
                    }
                    else if (d.Side == Bitmex.Client.Websocket.Responses.BitmexSide.Sell)
                    {
                        book.Asks.Delete(d.Id);
                    }                    
                }
            }
            else if (msg.Action == Bitmex.Client.Websocket.Responses.BitmexAction.Update)
            {
                for (int ii = 0; ii < msg.Data.Length; ii++)
                {
                    var d = msg.Data[ii];

                    var book = booksBySymbol[d.Symbol];

                    if (d.Size.HasValue)
                    {
                        if (d.Side == Bitmex.Client.Websocket.Responses.BitmexSide.Buy)
                        {
                            book.Bids.Update(d.Id, d.Size.Value);
                        }
                        else if (d.Side == Bitmex.Client.Websocket.Responses.BitmexSide.Sell)
                        {
                            book.Asks.Update(d.Id, d.Size.Value);
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
