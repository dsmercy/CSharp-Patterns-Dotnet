// PROBLEM: RealProductCatalogueService used directly — three issues:
//   1. No caching — same product fetched 5 times = 5 slow external API calls.
//   2. Eager construction — heavy connection setup runs at startup even if no
//      product is ever requested in this particular code path.
//   3. No access control — a guest user can call UpdatePriceAsync with no rejection.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: No proxy — direct access to RealProductCatalogueService ===");
Log.Information("");

// PROBLEM 1: Eager construction — heavy init fires immediately at startup.
Log.Information("--- Creating service at startup (heavy connection opens NOW) ---");
var service = new RealProductCatalogueService();   // ← slow constructor runs here
Log.Information("");

// PROBLEM 2: No caching — five requests for the same product hit the API five times.
Log.Information("--- Requesting product P001 five times (should cache after first) ---");
for (var i = 1; i <= 5; i++)
{
    var p = await service.GetProductAsync("P001");
    Log.Information("  Got '{Name}' — £{Price:F2}", p.Name, p.Price);
}
Log.Information("  >>> 5 'External API call' lines above — all unnecessary after the first.");
Log.Information("");

// PROBLEM 3: No access control — guest can update prices.
Log.Information("--- Guest user calling UpdatePriceAsync (should be Admin only) ---");
var guestRole = "Guest";
Log.Warning("  Current role: {Role}", guestRole);
await service.UpdatePriceAsync("P001", 9.99m);     // ← no role check, succeeds silently
Log.Warning("  Price updated by guest! No access control enforced.");
Log.Information("");
Log.Information(">>> Fix: Proxy wraps the real service — adds caching, lazy init, and");
Log.Information(">>>      access control without touching RealProductCatalogueService.");

// ─── Domain ───────────────────────────────────────────────────────────────────

public record Product(string ProductId, string Name, decimal Price, string Category);

public class RealProductCatalogueService
{
    public RealProductCatalogueService()
    {
        // Simulates a slow setup: opening a connection, loading config, etc.
        Thread.Sleep(200);
        Log.Warning("  [Real] HEAVY INIT — connection pool opened, config loaded (200ms)");
    }

    public async Task<Product> GetProductAsync(string productId)
    {
        await Task.Delay(80); // simulate external API round-trip
        Log.Information("  [Real] External API call for product {Id}", productId);
        return productId switch
        {
            "P001" => new Product("P001", "Wireless Mouse",  29.99m, "Electronics"),
            "P002" => new Product("P002", "USB-C Hub",       89.99m, "Electronics"),
            _      => new Product(productId, "Unknown Product", 0m, "Unknown")
        };
    }

    public async Task<IList<Product>> SearchAsync(string query, int page)
    {
        await Task.Delay(120);
        Log.Information("  [Real] External API search for '{Query}' page {Page}", query, page);
        return [new Product("P001", "Wireless Mouse", 29.99m, "Electronics")];
    }

    public async Task UpdatePriceAsync(string productId, decimal newPrice)
    {
        await Task.Delay(50);
        Log.Information("  [Real] Price updated — {Id} → £{Price:F2}", productId, newPrice);
    }
}
