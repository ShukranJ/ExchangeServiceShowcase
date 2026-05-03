namespace ExchangeServiceShowcase.Models;

public sealed class ExchangeRateDto
{
    public string Code { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Mid { get; set; }
    public decimal Ask { get; set; }
    public decimal Bid { get; set; }
    public DateTime EffectiveDate { get; set; }
}
