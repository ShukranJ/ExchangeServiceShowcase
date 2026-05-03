using ExchangeServiceShowcase.Models;

namespace ExchangeServiceShowcase.Services;

public interface INbpApiService
{
    Task<ExchangeRateDto> GetCurrentRateAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<HistoricalRatePoint>> GetHistoryAsync(string code, int days, CancellationToken cancellationToken = default);
}
