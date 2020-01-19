using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTrader
{
    class StrategySummary
    {
        public string Environment { get; set; }
        public string State { get; set; }
        public decimal WalletBalance { get; set; }
        public decimal AvailableMargin { get; set; }
        public decimal RealizedPnl { get; set; }
        public decimal UnrealizedPnl { get; set; }
        public decimal Leverage { get; set; }
        public decimal UsedMargin { get; set; }
    }
}
