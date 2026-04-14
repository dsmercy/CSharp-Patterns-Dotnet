using Proxy.App.Models;
using Serilog;

namespace Proxy.App;

// The Real Subject — makes slow external API calls.
// Its constructor simulates a heavy connection setup (connection pool, config loading).
// Proxies wrap this; clients never instantiate it directly.
public class RealProductCatalogueService : IProductCatalogueService
{
    private static readonly Dictionary<string, Product> _catalogue = new()
    {
        ["P001"] = new Product("P001", "Wireless Mouse",   29.99m, "Electronics"),
        ["P002"] = new Product("P002", "USB-C Hub",        89.99m, "Electronics"),
        ["P003"] = new Product("P003", "Laptop Stand",     34.99m, "Accessories"),
        ["P004"] = new Product("P004", "Mechanical Keyboard", 129.99m, "Electronics"),
    };

    public RealProductCatalogueService()
    {
        Thread.Sleep(200); // simulate connection pool setup + config loading
        Log.Warning("  [Real] HEAVY INIT — connection pool opened (200ms startup cost)");
    }

    public async Task<Product> GetProductAsync(string productId)
    {
        await Task.Delay(80); // simulate external API round-trip
        Log.Information("  [Real] External API call — GetProduct({Id})", productId);
        return _catalogue.TryGetValue(productId, out var p)
            ? p
            : throw new KeyNotFoundException($"Product '{productId}' not found.");
    }

    public async Task<IList<Product>> SearchAsync(string query, int page = 1)
    {
        await Task.Delay(120); // search is slower than a direct lookup
        Log.Information("  [Real] External API call — Search('{Query}', page {Page})", query, page);
        return _catalogue.Values
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task UpdatePriceAsync(string productId, decimal newPrice)
    {
        await Task.Delay(50);
        Log.Information("  [Real] Price updated — {Id} → £{Price:F2}", productId, newPrice);
    }
}
