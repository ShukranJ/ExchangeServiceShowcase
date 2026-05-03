using ExchangeServiceShowcase.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeServiceShowcase.Controllers;

[ApiController]
[Route("api/rates")]
public sealed class RatesController : ControllerBase
{
    private readonly INbpApiService _nbpApi;

    public RatesController(INbpApiService nbpApi)
    {
        _nbpApi = nbpApi;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetRate(string code, CancellationToken cancellationToken)
    {
        var rate = await _nbpApi.GetCurrentRateAsync(code, cancellationToken);
        return Ok(new
        {
            code = rate.Code,
            currency = rate.Currency,
            mid = rate.Mid,
            ask = rate.Ask,
            bid = rate.Bid,
            effectiveDate = rate.EffectiveDate.ToString("yyyy-MM-dd")
        });
    }

    [HttpGet("history/{code}")]
    public async Task<IActionResult> GetHistory(string code, [FromQuery] int days = 7, CancellationToken cancellationToken = default)
    {
        var history = await _nbpApi.GetHistoryAsync(code, days, cancellationToken);
        return Ok(history.Select(point => new
        {
            date = point.Date.ToString("yyyy-MM-dd"),
            currency = point.CurrencyCode,
            rate = point.Rate
        }));
    }
}
