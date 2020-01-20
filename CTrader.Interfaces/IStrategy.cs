using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTrader.Interfaces
{
    public interface IStrategy
    {
        void Start();
        void Stop();
        void Rebalance();

        IStrategySummary GetSummary();

    }
}
