namespace ExchangeServiceShowcase.Models;

public sealed class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public List<CurrencyBalance> CurrencyBalances { get; set; } = [];
    public List<ExchangeTransaction> Transactions { get; set; } = [];
}
