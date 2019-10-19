using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MDS
{
    public class Level
    {
        private long id;
        private decimal price;
        private decimal quantity;
        public int height;
        public Level left, right;

        public Level(long identifier, decimal p, decimal q)
        {
            id = identifier;
            price = p;
            quantity = q;
            height = 1;
        }

        public long Id { get { return id; } }
        public decimal Price { get { return price; } }
        public decimal Quantity 
        { 
            get 
            {
                object retval = 0;
                Interlocked.Exchange(ref retval, quantity);
                return (decimal)retval;
            } 
            set 
            {
                object val = 0;
                Interlocked.Exchange(ref val, value);
                quantity = (decimal)val;
            } 
        }
    }

    public class Levels
    {
        private Level left = null;
        private Level right = null;
        private Dictionary<long, Level> levelById = new Dictionary<long, Level>();
        private SortedDictionary<decimal, Level> levelByPrice = new SortedDictionary<decimal, Level>();

        public decimal Cheapest { get { Level lvl = null; Interlocked.Exchange(ref lvl, left); if (lvl != null) return lvl.Price; else return -1; } }
        public decimal Expensive { get { Level lvl = null; Interlocked.Exchange(ref lvl, right); if (lvl != null) return lvl.Price; else return -1; } }

        public void Add(long msgId, decimal price, decimal quantity)
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

        public void Update(long msgId, decimal quantity)
        {
            if (levelById.ContainsKey(msgId))
                levelById[msgId].Quantity = quantity;
        }

        public void Delete(long msgId)
        {
            if (!levelById.ContainsKey(msgId))
                return;

            var lvl = levelById[msgId];
            levelById.Remove(msgId);
            levelByPrice.Remove(lvl.Price);

            if (lvl == left)
            {
                var keys = levelByPrice.Keys.ToList();

                if (keys.Count > 0)
                    Interlocked.Exchange(ref left, levelByPrice[keys.First()]);
                else
                {
                    Interlocked.Exchange(ref left, null);
                }
            }

            if (lvl == right)
            {
                var keys = levelByPrice.Keys.ToList();
                if (keys.Count > 0)
                    Interlocked.Exchange(ref right, levelByPrice[keys.Last()]);
                else
                {
                    Interlocked.Exchange(ref right, null);
                }
            }
        }
    }

    public class OrderBook
    {
        public Levels Bids = new Levels();
        public Levels Asks = new Levels();
    }
}
