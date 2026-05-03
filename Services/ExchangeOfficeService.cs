using ExchangeServiceShowcase.Data;
using ExchangeServiceShowcase.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeServiceShowcase.Services;

public sealed class ExchangeOfficeService : IExchangeOfficeService
{
    private static readonly string[] FeaturedCodes = ["USD", "EUR", "GBP", "CHF"];
    private const string Pln = "PLN";
    private readonly ApplicationDbContext _db;
    private readonly INbpApiService _nbpApi;

    public ExchangeOfficeService(ApplicationDbContext db, INbpApiService nbpApi)
    {
        _db = db;
        _nbpApi = nbpApi;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(int? accountId, string selectedCurrency, int historyDays, CancellationToken cancellationToken = default)
    {
        var model = new DashboardViewModel
        {
            CurrentAccountId = accountId,
            SelectedCurrency = string.IsNullOrWhiteSpace(selectedCurrency) ? "USD" : selectedCurrency.ToUpperInvariant(),
            HistoryDays = Math.Clamp(historyDays, 1, 30)
        };

        Account? account = null;
        if (accountId.HasValue)
        {
            account = await _db.Accounts
                .Include(a => a.CurrencyBalances)
                .Include(a => a.Transactions.OrderByDescending(t => t.DateUtc))
                .SingleOrDefaultAsync(a => a.Id == accountId.Value, cancellationToken);
        }

        model.Account = account;
        if (account is not null)
        {
            model.Username = account.Name;
        }

        foreach (var code in FeaturedCodes)
        {
            var current = await _nbpApi.GetCurrentRateAsync(code, cancellationToken);
            var history = await _nbpApi.GetHistoryAsync(code, 2, cancellationToken);
            var points = history.ToArray();
            var changePercent = points.Length >= 2 && points[1].Rate != 0
                ? decimal.Round(((points[0].Rate - points[1].Rate) / points[1].Rate) * 100m, 2)
                : 0m;
            var ownedAmount = account?.CurrencyBalances.SingleOrDefault(b => b.CurrencyCode == code)?.Amount ?? 0m;

            model.RateCards.Add(new RateCardViewModel
            {
                Code = code,
                Currency = current.Currency,
                Mid = current.Mid,
                ChangePercent = changePercent,
                OwnedAmount = ownedAmount
            });
        }

        model.SelectedRate = await _nbpApi.GetCurrentRateAsync(model.SelectedCurrency, cancellationToken);
        model.History = (await _nbpApi.GetHistoryAsync(model.SelectedCurrency, model.HistoryDays, cancellationToken)).ToList();

        return model;
    }

    public async Task<int> CreateAccountAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        var normalized = username.Trim();
        if (await _db.Accounts.AnyAsync(a => a.Name.ToLower() == normalized.ToLower(), cancellationToken))
        {
            throw new InvalidOperationException($"Account '{normalized}' already exists.");
        }

        var account = new Account
        {
            Name = normalized,
            CreatedAtUtc = DateTime.UtcNow,
            CurrencyBalances =
            [
                new CurrencyBalance
                {
                    CurrencyCode = Pln,
                    Amount = 0m
                }
            ]
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync(cancellationToken);
        return account.Id;
    }

    public async Task TopUpAsync(int accountId, decimal amountPln, CancellationToken cancellationToken = default)
    {
        if (amountPln <= 0m)
        {
            throw new InvalidOperationException("Top up amount must be positive.");
        }

        var account = await GetAccountAsync(accountId, cancellationToken);
        GetOrCreateBalance(account, Pln).Amount += amountPln;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task BuyCurrencyAsync(int accountId, string currencyCode, decimal amountPln, CancellationToken cancellationToken = default)
    {
        if (amountPln <= 0m)
        {
            throw new InvalidOperationException("Buy amount must be positive.");
        }

        var account = await GetAccountAsync(accountId, cancellationToken);
        var rate = await _nbpApi.GetCurrentRateAsync(currencyCode, cancellationToken);
        var plnBalance = GetOrCreateBalance(account, Pln);

        if (plnBalance.Amount < amountPln)
        {
            throw new InvalidOperationException("Insufficient PLN balance.");
        }

        var foreignAmount = decimal.Round(amountPln / rate.Ask, 4, MidpointRounding.AwayFromZero);
        plnBalance.Amount -= amountPln;
        GetOrCreateBalance(account, rate.Code).Amount += foreignAmount;

        _db.Transactions.Add(new ExchangeTransaction
        {
            AccountId = account.Id,
            DateUtc = DateTime.UtcNow,
            Type = "BUY",
            FromCurrency = Pln,
            ToCurrency = rate.Code,
            Amount = amountPln,
            Rate = rate.Ask,
            ResultAmount = foreignAmount
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SellCurrencyAsync(int accountId, string currencyCode, decimal amountCurrency, CancellationToken cancellationToken = default)
    {
        if (amountCurrency <= 0m)
        {
            throw new InvalidOperationException("Sell amount must be positive.");
        }

        var account = await GetAccountAsync(accountId, cancellationToken);
        var rate = await _nbpApi.GetCurrentRateAsync(currencyCode, cancellationToken);
        var foreignBalance = GetOrCreateBalance(account, rate.Code);

        if (foreignBalance.Amount < amountCurrency)
        {
            throw new InvalidOperationException($"Insufficient {rate.Code} balance.");
        }

        var plnAmount = decimal.Round(amountCurrency * rate.Bid, 2, MidpointRounding.AwayFromZero);
        foreignBalance.Amount -= amountCurrency;
        GetOrCreateBalance(account, Pln).Amount += plnAmount;

        _db.Transactions.Add(new ExchangeTransaction
        {
            AccountId = account.Id,
            DateUtc = DateTime.UtcNow,
            Type = "SELL",
            FromCurrency = rate.Code,
            ToCurrency = Pln,
            Amount = amountCurrency,
            Rate = rate.Bid,
            ResultAmount = plnAmount
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Account> GetAccountAsync(int id, CancellationToken cancellationToken)
    {
        var account = await _db.Accounts
            .Include(a => a.CurrencyBalances)
            .Include(a => a.Transactions)
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        return account ?? throw new InvalidOperationException("Account was not found.");
    }

    private static CurrencyBalance GetOrCreateBalance(Account account, string code)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var balance = account.CurrencyBalances.SingleOrDefault(b => b.CurrencyCode == normalized);
        if (balance is not null)
        {
            return balance;
        }

        balance = new CurrencyBalance
        {
            AccountId = account.Id,
            CurrencyCode = normalized,
            Amount = 0m
        };

        account.CurrencyBalances.Add(balance);
        return balance;
    }
}
