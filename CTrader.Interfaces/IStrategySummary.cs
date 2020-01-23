namespace CTrader.Interfaces
{
    public interface IStrategySummary
    {
        string Environment { get; set; }
        string State { get; set; }
        decimal WalletBalance { get; set; }
        decimal BalanceMargin { get; set; }
        decimal RealizedPnl { get; set; }
        decimal UnrealizedPnl { get; set; }
        decimal Leverage { get; set; }
        decimal UsedMargin { get; set; }
    }
}
