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
    }

    public class XBTUSD : IInstrProp
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
            throw new NotImplementedException("Reference does not exist for ETH");
        }

    }

    public class XCFUSD : IInstrProp
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
            return ".BETHXBT30M";
        }
    }

}
