using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MDS
{
    public class Level
    {
        private long id;
        private double price;
        private long quantity;
        public int height;
        public Level left, right;

        public Level(long identifier, double p, long q)
        {
            id = identifier;
            price = p;
            quantity = q;
            height = 1;
        }

        public long Id { get { return id; } }
        public double Price { get { return price; } }
        public long Quantity { get { return Interlocked.Read(ref quantity); } set { Interlocked.Exchange(ref quantity, value); } }
    }

    public class Levels
    {
        private Level left = null;
        private Level right = null;
        private Dictionary<long, Level> levelById = new Dictionary<long, Level>();
        private SortedDictionary<double, Level> levelByPrice = new SortedDictionary<double, Level>();

        public double Cheapest { get { Level lvl = null; Interlocked.Exchange(ref lvl, left); if (lvl != null) return lvl.Price; else return -1; } }
        public double Expensive { get { Level lvl = null; Interlocked.Exchange(ref lvl, right); if (lvl != null) return lvl.Price; else return -1; } }

        public void Add(long msgId, double price, long quantity)
        {
            var lvl = new Level(msgId, price, quantity);
            levelById[msgId] = lvl;
            levelByPrice[price] = lvl;

            if (levelById.Count == 1)
            {
                Interlocked.Exchange(ref left, lvl);
                Interlocked.Exchange(ref right, lvl);
            }
            else if (price < left.Price)
            {
                Interlocked.Exchange(ref left, lvl);
            }
            else if (price > right.Price)
            {
                Interlocked.Exchange(ref right, lvl);
            }
        }

        public void Update(long msgId, long quantity)
        {
            levelById[msgId].Quantity = quantity;
        }

        public void Delete(long msgId)
        {
            var lvl = levelById[msgId];
            levelById.Remove(msgId);
            levelByPrice.Remove(lvl.Price);

            if (lvl == left)
            {
                var keys = levelByPrice.Keys.ToList();
                Interlocked.Exchange(ref left, levelByPrice[keys.First()]);
            }

            if (lvl == right)
            {
                var keys = levelByPrice.Keys.ToList();
                Interlocked.Exchange(ref right, levelByPrice[keys.Last()]);
            }
        }
    }

    public class OrderBook
    {
        public Levels Bids = new Levels();
        public Levels Asks = new Levels();
    }
}
