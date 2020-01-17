using Bitmex.NET.Dtos;
using Bitmex.NET.Dtos.Socket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeService
{
    public class MDS
    {
        private readonly Dictionary<string, OrderBook10Dto> notifications = new Dictionary<string, OrderBook10Dto>();
        private readonly List<object> subscriptions = new List<object>();
        private readonly ConcurrentDictionary<string, Table> books = new ConcurrentDictionary<string, Table>();
        private volatile bool stop = false;
        private readonly Dictionary<string, decimal> instrumentData = new Dictionary<string, decimal>();

        public void Start()
        {
            Thread t = new Thread(new ThreadStart(OnStart));
            t.Start();
        }

        public void Stop()
        {
            stop = true;
        }

        public void Post(IEnumerable<InstrumentDto> dtos)
        {
            lock (instrumentData)
            {
                foreach (var d in dtos)
                {
                    if (d.LastPrice.HasValue)
                        instrumentData[d.Symbol] = d.LastPrice.Value;
                }
            }
        }

        public void Post(IEnumerable<OrderBook10Dto> dtos)
        {
            lock(notifications)
            {
                foreach(var d in dtos)
                {
                    notifications[d.Symbol] = d;
                }
            }
        }

        private void OnStart()
        {
            while(!stop)
            {
                lock(notifications)
                {
                    foreach (var d in notifications.Values)
                    {
                        var tbl = books[d.Symbol];
                        lock (tbl)
                        {
                            tbl.Bids.Clear();
                            ReverseSort(d.Bids, 0);
                            foreach (var bid in d.Bids)
                            {
                                tbl.Bids.Add(bid[0]);
                            }

                            tbl.Asks.Clear();
                            Sort(d.Asks, 0);
                            foreach (var ask in d.Asks)
                            {
                                tbl.Asks.Add(ask[0]);
                            }
                        }
                    }

                    notifications.Clear();
                }

                Thread.Sleep(200);
            }
        }


        private static void Sort<T>(T[][] data, int col)
        {
            Comparer<T> comparer = Comparer<T>.Default;
            Array.Sort<T[]>(data, (x, y) => comparer.Compare(x[col], y[col]));
        }

        private static void ReverseSort<T>(T[][] data, int col)
        {
            Comparer<T> comparer = Comparer<T>.Default;
            Array.Sort<T[]>(data, (x, y) => comparer.Compare(y[col], x[col]));
        }

        public object[] GetSubscriptions()
        {
            return subscriptions.ToArray();
        }

        public object[] GetInstrs()
        {
            return instrumentData.Keys.ToList().ToArray<object>();
        }

        public decimal GetInstrLast(string symbol)
        {
            decimal retval = 0;

            lock(instrumentData)
            {
                if (instrumentData.ContainsKey(symbol))
                {
                    retval = instrumentData[symbol];
                }
            }

            return retval;
        }

        public decimal GetBestBid(string symbol)
        {
            Table mkt = books[symbol];

            lock(mkt)
            {
                if (mkt.Bids.Count > 0)
                    return mkt.Bids[0];
                else
                    return -1;
            }
        }

        public decimal GetBestAsk(string symbol)
        {
            Table mkt = books[symbol];

            lock(mkt)
            {
                if (mkt.Asks.Count > 0)
                    return mkt.Asks[0];
                else
                    return -1;
            }
        }

        public Table GetBook(string symbol)
        {
            Table tbl = new Table
            {
                Asks = new List<decimal>(),
                Bids = new List<decimal>()
            };

            Table mkt = books[symbol];

            lock (mkt)
            {
                tbl.Asks.AddRange(mkt.Asks);
                tbl.Bids.AddRange(mkt.Bids);
            }

            return tbl;
        }
        public void Register(HashSet<string> symbols)
        {
            foreach (var symbol in symbols)
            {
                subscriptions.Add(symbol);
                books[symbol] = new Table
                {
                    Bids = new List<decimal>(),
                    Asks = new List<decimal>()
                };
            }
        }

        public void RegisterInstruments(HashSet<string> instrs)
        {
            foreach(var symbol in instrs)
            {
                instrumentData[symbol] = 0;
            }
        }
    }
}
