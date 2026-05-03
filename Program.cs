using ExchangeServiceShowcase.Data;
using ExchangeServiceShowcase.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5068");
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ExchangeDb")));
builder.Services.AddHttpClient<INbpApiService, NbpApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.nbp.pl/api/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IExchangeOfficeService, ExchangeOfficeService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
