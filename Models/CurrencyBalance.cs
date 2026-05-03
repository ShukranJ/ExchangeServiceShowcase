namespace ExchangeServiceShowcase.Models;

public sealed class CurrencyBalance
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Account? Account { get; set; }
}
