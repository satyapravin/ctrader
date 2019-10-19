using System;
using System.Collections.Generic;
using System.Text;

namespace Executor
{
    public interface InstrProp
    {
        string Symbol();
        decimal GetQuantity(decimal posval, decimal bid, decimal ask);

        decimal GetPositionValue(decimal quantity, decimal bid, decimal ask);
    }

    public class XBTUSD : InstrProp
    {
        public decimal GetPositionValue(decimal quantity, decimal bid, decimal ask)
        {
            decimal retval = quantity;

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

        public decimal GetQuantity(decimal posval, decimal bid, decimal ask)
        {
            if (posval > 0)
            {
                return Math.Round(posval * bid, 0);
            }
            else
            {
                return Math.Round(posval * ask, 0);
            }
        }

        public string Symbol()
        {
            return "XBTUSD";
        }
    }

    public class ETHUSD : InstrProp
    {
        public decimal GetPositionValue(decimal quantity, decimal bid, decimal ask)
        {
            decimal retval = 0.000001m;

            if (quantity > 0)
            {
                return retval * quantity * bid;
            }
            else
            {
                return retval * quantity * ask;
            }
        }

        public decimal GetQuantity(decimal posval, decimal bid, decimal ask)
        {
            if (posval > 0)
            {
                return Math.Round((posval / 0.000001m) / bid, 0);
            }
            else
            {
                return Math.Round((posval / 0.000001m) / ask, 0);
            }
        }

        public string Symbol()
        {
            return "ETHUSD";
        }
    }

    public class XCFUSD : InstrProp
    {
        public decimal GetPositionValue(decimal quantity, decimal bid, decimal ask)
        {
            decimal retval = quantity;

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

        public decimal GetQuantity(decimal posval, decimal bid, decimal ask)
        {
            if (posval > 0)
            {
                return Math.Round(posval / bid, 0);
            }
            else
            {
                return Math.Round(posval / ask, 0);
            }
        }

        public string Symbol()
        {
            return "ETHZ19";
        }
    }
}
