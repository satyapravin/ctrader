using CTrader.Interfaces;

namespace CTrader
{
    public class StrategySummary : IStrategySummary
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
