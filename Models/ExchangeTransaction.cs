namespace ExchangeServiceShowcase.Models;

public sealed class ExchangeTransaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime DateUtc { get; set; }
    public string Type { get; set; } = string.Empty;
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Rate { get; set; }
    public decimal ResultAmount { get; set; }
    public Account? Account { get; set; }
}
