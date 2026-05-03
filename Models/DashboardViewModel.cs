namespace ExchangeServiceShowcase.Models;

public sealed class DashboardViewModel
{
    public string StatusMessage { get; set; } = "Create an account to start using the exchange office.";
    public string Username { get; set; } = "John Doe";
    public int? CurrentAccountId { get; set; }
    public string SelectedCurrency { get; set; } = "USD";
    public string BuyAmountPln { get; set; } = "1000";
    public string SellAmountCurrency { get; set; } = "200";
    public string TopUpAmount { get; set; } = "1000";
    public int HistoryDays { get; set; } = 7;
    public Account? Account { get; set; }
    public ExchangeRateDto? SelectedRate { get; set; }
    public List<RateCardViewModel> RateCards { get; set; } = [];
    public List<HistoricalRatePoint> History { get; set; } = [];
}
