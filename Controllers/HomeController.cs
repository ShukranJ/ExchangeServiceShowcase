using ExchangeServiceShowcase.Models;
using ExchangeServiceShowcase.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeServiceShowcase.Controllers;

public sealed class HomeController : Controller
{
    private const string AccountIdKey = "account-id";
    private const string CurrencyKey = "currency-code";
    private readonly IExchangeOfficeService _exchangeOfficeService;

    public HomeController(IExchangeOfficeService exchangeOfficeService)
    {
        _exchangeOfficeService = exchangeOfficeService;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index(int? accountId = null, string currency = "USD", int days = 7)
    {
        var effectiveAccountId = accountId ?? GetAccountId();
        var effectiveCurrency = string.IsNullOrWhiteSpace(currency) ? HttpContext.Session.GetString(CurrencyKey) ?? "USD" : currency;

        var model = await _exchangeOfficeService.GetDashboardAsync(effectiveAccountId, effectiveCurrency, days);
        if (TempData.TryGetValue("StatusMessage", out var status) && status is string message)
        {
            model.StatusMessage = message;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(DashboardViewModel input)
    {
        try
        {
            var accountId = await _exchangeOfficeService.CreateAccountAsync(input.Username);
            SaveSession(accountId, input.SelectedCurrency);
            TempData["StatusMessage"] = $"Account '{input.Username}' created successfully.";
            return RedirectToAction(nameof(Index), new { accountId, currency = input.SelectedCurrency, days = input.HistoryDays });
        }
        catch (Exception ex)
        {
            SaveSession(input.CurrentAccountId, input.SelectedCurrency);
            TempData["StatusMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { accountId = input.CurrentAccountId, currency = input.SelectedCurrency, days = input.HistoryDays });
        }
    }

    [HttpPost]
    public IActionResult LoadAccount(DashboardViewModel input)
    {
        SaveSession(input.CurrentAccountId, input.SelectedCurrency);
        TempData["StatusMessage"] = input.CurrentAccountId.HasValue
            ? $"Loaded account #{input.CurrentAccountId.Value}."
            : "Provide an account ID first.";
        return RedirectToAction(nameof(Index), new { accountId = input.CurrentAccountId, currency = input.SelectedCurrency, days = input.HistoryDays });
    }

    [HttpPost]
    public async Task<IActionResult> TopUp(DashboardViewModel input)
    {
        try
        {
            await _exchangeOfficeService.TopUpAsync(RequireAccount(input.CurrentAccountId), ParseDecimal(input.TopUpAmount));
            TempData["StatusMessage"] = $"Added {input.TopUpAmount} PLN.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = ex.Message;
        }

        SaveSession(input.CurrentAccountId, input.SelectedCurrency);
        return RedirectToAction(nameof(Index), new { accountId = input.CurrentAccountId, currency = input.SelectedCurrency, days = input.HistoryDays });
    }

    [HttpPost]
    public async Task<IActionResult> Buy(DashboardViewModel input)
    {
        try
        {
            await _exchangeOfficeService.BuyCurrencyAsync(RequireAccount(input.CurrentAccountId), input.SelectedCurrency, ParseDecimal(input.BuyAmountPln));
            TempData["StatusMessage"] = $"Exchanged {input.BuyAmountPln} PLN to {input.SelectedCurrency}.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = ex.Message;
        }

        SaveSession(input.CurrentAccountId, input.SelectedCurrency);
        return RedirectToAction(nameof(Index), new { accountId = input.CurrentAccountId, currency = input.SelectedCurrency, days = input.HistoryDays });
    }

    [HttpPost]
    public async Task<IActionResult> Sell(DashboardViewModel input)
    {
        try
        {
            await _exchangeOfficeService.SellCurrencyAsync(RequireAccount(input.CurrentAccountId), input.SelectedCurrency, ParseDecimal(input.SellAmountCurrency));
            TempData["StatusMessage"] = $"Sold {input.SellAmountCurrency} {input.SelectedCurrency}.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = ex.Message;
        }

        SaveSession(input.CurrentAccountId, input.SelectedCurrency);
        return RedirectToAction(nameof(Index), new { accountId = input.CurrentAccountId, currency = input.SelectedCurrency, days = input.HistoryDays });
    }

    [HttpPost]
    public IActionResult ChangeCurrency(DashboardViewModel input)
    {
        SaveSession(input.CurrentAccountId, input.SelectedCurrency);
        return RedirectToAction(nameof(Index), new { accountId = input.CurrentAccountId, currency = input.SelectedCurrency, days = input.HistoryDays });
    }

    private void SaveSession(int? accountId, string currency)
    {
        if (accountId.HasValue)
        {
            HttpContext.Session.SetInt32(AccountIdKey, accountId.Value);
        }

        HttpContext.Session.SetString(CurrencyKey, currency);
    }

    private int? GetAccountId() => HttpContext.Session.GetInt32(AccountIdKey);

    private static int RequireAccount(int? accountId)
    {
        if (!accountId.HasValue)
        {
            throw new InvalidOperationException("Create or load an account first.");
        }

        return accountId.Value;
    }

    private static decimal ParseDecimal(string raw)
    {
        if (decimal.TryParse(raw, out var currentCulture))
        {
            return currentCulture;
        }

        if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var invariant))
        {
            return invariant;
        }

        throw new InvalidOperationException("Enter a valid amount.");
    }
}
