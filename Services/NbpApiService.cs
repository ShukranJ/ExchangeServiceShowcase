using System.Net;
using System.Net.Http.Json;
using ExchangeServiceShowcase.Models;

namespace ExchangeServiceShowcase.Services;

public sealed class NbpApiService : INbpApiService
{
    private readonly HttpClient _httpClient;

    public NbpApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExchangeRateDto> GetCurrentRateAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = Normalize(code);
        var cResponse = await _httpClient.GetAsync($"exchangerates/rates/c/{normalizedCode}/?format=json", cancellationToken);
        if (cResponse.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Currency '{normalizedCode}' was not found in the NBP API.");
        }

        cResponse.EnsureSuccessStatusCode();
        var cPayload = await cResponse.Content.ReadFromJsonAsync<TableCResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("NBP returned an empty current rate response.");

        var aResponse = await _httpClient.GetAsync($"exchangerates/rates/a/{normalizedCode}/?format=json", cancellationToken);
        aResponse.EnsureSuccessStatusCode();
        var aPayload = await aResponse.Content.ReadFromJsonAsync<TableAResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("NBP returned an empty table A response.");

        return new ExchangeRateDto
        {
            Code = normalizedCode,
            Currency = cPayload.Currency,
            Ask = cPayload.Rates[0].Ask,
            Bid = cPayload.Rates[0].Bid,
            Mid = aPayload.Rates[0].Mid,
            EffectiveDate = cPayload.Rates[0].EffectiveDate
        };
    }

    public async Task<IReadOnlyCollection<HistoricalRatePoint>> GetHistoryAsync(string code, int days, CancellationToken cancellationToken = default)
    {
        var normalizedCode = Normalize(code);
        var clampedDays = Math.Clamp(days, 1, 30);
        var response = await _httpClient.GetAsync($"exchangerates/rates/a/{normalizedCode}/last/{clampedDays}/?format=json", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Currency '{normalizedCode}' was not found in the NBP API.");
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<HistoryResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("NBP returned an empty history response.");

        return payload.Rates
            .OrderByDescending(rate => rate.EffectiveDate)
            .Select(rate => new HistoricalRatePoint
            {
                Date = rate.EffectiveDate,
                CurrencyCode = normalizedCode,
                Rate = rate.Mid
            })
            .ToArray();
    }

    private static string Normalize(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("Currency code is required.");
        }

        return code.Trim().ToUpperInvariant();
    }

    private sealed class TableCResponse
    {
        public string Currency { get; set; } = string.Empty;
        public List<TableCRate> Rates { get; set; } = [];
    }

    private sealed class TableCRate
    {
        public DateTime EffectiveDate { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }

    private sealed class TableAResponse
    {
        public List<TableARate> Rates { get; set; } = [];
    }

    private sealed class TableARate
    {
        public decimal Mid { get; set; }
    }

    private sealed class HistoryResponse
    {
        public List<HistoryRate> Rates { get; set; } = [];
    }

    private sealed class HistoryRate
    {
        public DateTime EffectiveDate { get; set; }
        public decimal Mid { get; set; }
    }
}
