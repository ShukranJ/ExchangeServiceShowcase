namespace ExchangeServiceShowcase.Models;

public sealed class HistoricalRatePoint
{
    public DateTime Date { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}
