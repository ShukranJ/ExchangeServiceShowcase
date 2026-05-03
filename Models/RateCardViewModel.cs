namespace ExchangeServiceShowcase.Models;

public sealed class RateCardViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Mid { get; set; }
    public decimal ChangePercent { get; set; }
    public decimal OwnedAmount { get; set; }
}
