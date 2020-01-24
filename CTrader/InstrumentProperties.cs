using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTrader
{
    public interface IInstrProp
    {
        string Symbol();

        string Reference();
        decimal GetQuantity(decimal posval, decimal bid, decimal ask);

        decimal GetPositionValue(decimal quantity, decimal bid, decimal ask);

        decimal GetPnl(decimal quantity, decimal current, decimal previous);
    }

    public class XBTUSD : IInstrProp
    {
        public decimal GetPnl(decimal quantity, decimal current, decimal previous)
        {
            return (1.0m / previous - 1.0m / current) * quantity;
        }
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
                return posval * bid;
            }
            else
            {
                return posval * ask;
            }
        }

        public string Symbol()
        {
            return "XBTUSD";
        }

        public string Reference()
        {
            return ".BXBT";
        }
    }

    public class ETHUSD : IInstrProp
    {
        public decimal GetPnl(decimal quantity, decimal current, decimal previous)
        {
            return 0.000001m * (current - previous) * quantity;
        }
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
                return (posval / 0.000001m) / bid;
            }
            else
            {
                return (posval / 0.000001m) / ask;
            }
        }

        public string Symbol()
        {
            return "ETHUSD";
        }


        public string Reference()
        {
            return ".BETH";
        }

    }

    public class XCFUSD : IInstrProp
    {
        public decimal GetPnl(decimal quantity, decimal current, decimal previous)
        {
            return quantity * (current - previous);
        }
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
                return posval / bid;
            }
            else
            {
                return posval / ask;
            }
        }

        public string Symbol()
        {
            return "ETHH20";
        }

        public string Reference()
        {
            //return ".BETHXBT30M";
            throw new NotImplementedException();
        }
    }

}
