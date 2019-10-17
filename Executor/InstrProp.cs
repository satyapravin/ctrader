using System;
using System.Collections.Generic;
using System.Text;

namespace Executor
{
    public interface InstrProp
    {
        string Symbol();
        long GetQuantity(double posval, double bid, double ask);

        double GetPositionValue(long quantity, double bid, double ask);
    }

    public class XBTUSD : InstrProp
    {
        public double GetPositionValue(long quantity, double bid, double ask)
        {
            double retval = quantity;

            if (quantity > 0)
            {
                retval /= bid;
            }
            else
            {
                retval /= ask;
            }

            return retval;
        }

        public long GetQuantity(double posval, double bid, double ask)
        {
            if (posval > 0)
            {
                return (long)(posval * bid);
            }
            else
            {
                return (long)(posval * ask);
            }
        }

        public string Symbol()
        {
            return "XBTUSD";
        }
    }

    public class ETHUSD : InstrProp
    {
        public double GetPositionValue(long quantity, double bid, double ask)
        {
            double retval = 0.000001;

            if (quantity > 0)
            {
                return retval * quantity * bid;
            }
            else
            {
                return retval * quantity * ask;
            }
        }

        public long GetQuantity(double posval, double bid, double ask)
        {
            if (posval > 0)
            {
                return (long)Math.Round((posval / 0.000001) / bid, 0);
            }
            else
            {
                return (long)Math.Round((posval / 0.000001) / ask, 0);
            }
        }

        public string Symbol()
        {
            return "ETHUSD";
        }
    }

    public class XCFUSD : InstrProp
    {
        public double GetPositionValue(long quantity, double bid, double ask)
        {
            double retval = quantity;

            if (quantity > 0)
            {
                retval *= bid;
            }
            else
            {
                retval *= ask;
            }

            return retval;
        }

        public long GetQuantity(double posval, double bid, double ask)
        {
            if (posval > 0)
            {
                return (long)Math.Round(posval / bid, 0);
            }
            else
            {
                return (long)Math.Round(posval / ask, 0);
            }
        }

        public string Symbol()
        {
            return "ETHZ19";
        }
    }
}
