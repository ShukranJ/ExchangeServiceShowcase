using ExchangeServiceShowcase.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeServiceShowcase.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CurrencyBalance> CurrencyBalances => Set<CurrencyBalance>();
    public DbSet<ExchangeTransaction> Transactions => Set<ExchangeTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(account => account.Id);
            entity.HasIndex(account => account.Name).IsUnique();
            entity.Property(account => account.Name).HasMaxLength(64).IsRequired();
            entity.Property(account => account.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<CurrencyBalance>(entity =>
        {
            entity.HasKey(balance => balance.Id);
            entity.HasIndex(balance => new { balance.AccountId, balance.CurrencyCode }).IsUnique();
            entity.Property(balance => balance.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(balance => balance.Amount).HasPrecision(18, 4);
            entity.HasOne(balance => balance.Account)
                .WithMany(account => account.CurrencyBalances)
                .HasForeignKey(balance => balance.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExchangeTransaction>(entity =>
        {
            entity.HasKey(transaction => transaction.Id);
            entity.Property(transaction => transaction.Type).HasMaxLength(16).IsRequired();
            entity.Property(transaction => transaction.FromCurrency).HasMaxLength(3).IsRequired();
            entity.Property(transaction => transaction.ToCurrency).HasMaxLength(3).IsRequired();
            entity.Property(transaction => transaction.Amount).HasPrecision(18, 4);
            entity.Property(transaction => transaction.Rate).HasPrecision(18, 4);
            entity.Property(transaction => transaction.ResultAmount).HasPrecision(18, 4);
            entity.HasOne(transaction => transaction.Account)
                .WithMany(account => account.Transactions)
                .HasForeignKey(transaction => transaction.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
