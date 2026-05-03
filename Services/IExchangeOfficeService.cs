using ExchangeServiceShowcase.Models;

namespace ExchangeServiceShowcase.Services;

public interface IExchangeOfficeService
{
    Task<DashboardViewModel> GetDashboardAsync(int? accountId, string selectedCurrency, int historyDays, CancellationToken cancellationToken = default);
    Task<int> CreateAccountAsync(string username, CancellationToken cancellationToken = default);
    Task TopUpAsync(int accountId, decimal amountPln, CancellationToken cancellationToken = default);
    Task BuyCurrencyAsync(int accountId, string currencyCode, decimal amountPln, CancellationToken cancellationToken = default);
    Task SellCurrencyAsync(int accountId, string currencyCode, decimal amountCurrency, CancellationToken cancellationToken = default);
}
