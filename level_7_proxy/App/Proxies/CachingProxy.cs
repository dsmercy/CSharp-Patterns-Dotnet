using Proxy.App.Models;
using Serilog;

namespace Proxy.App.Proxies;

// PATTERN CONCEPT: Caching Proxy.
//
// Intercepts GetProductAsync and SearchAsync calls, returns a cached result
// if one exists and has not expired, otherwise delegates to the real service
// and stores the result. UpdatePriceAsync also invalidates the affected entry.
//
// This is the most common proxy in production C#.
// ASP.NET Core's IMemoryCache is exactly this pattern.
public class CachingProxy(IProductCatalogueService inner, TimeSpan ttl)
    : IProductCatalogueService
{
    private readonly Dictionary<string, (Product Product, DateTime Expiry)> _productCache = new();
    private readonly Dictionary<string, (IList<Product> Results, DateTime Expiry)> _searchCache = new();

    public async Task<Product> GetProductAsync(string productId)
    {
        if (_productCache.TryGetValue(productId, out var entry) && entry.Expiry > DateTime.UtcNow)
        {
            Log.Information("  [CachingProxy] Cache HIT  — GetProduct({Id})", productId);
            return entry.Product;
        }

        Log.Information("  [CachingProxy] Cache MISS — GetProduct({Id}), delegating to real service", productId);
        var product = await inner.GetProductAsync(productId);
        _productCache[productId] = (product, DateTime.UtcNow.Add(ttl));
        return product;
    }

    public async Task<IList<Product>> SearchAsync(string query, int page = 1)
    {
        var key = $"{query}::{page}";
        if (_searchCache.TryGetValue(key, out var entry) && entry.Expiry > DateTime.UtcNow)
        {
            Log.Information("  [CachingProxy] Cache HIT  — Search('{Query}', page {Page})", query, page);
            return entry.Results;
        }

        Log.Information("  [CachingProxy] Cache MISS — Search('{Query}', page {Page})", query, page);
        var results = await inner.SearchAsync(query, page);
        _searchCache[key] = (results, DateTime.UtcNow.Add(ttl));
        return results;
    }

    public async Task UpdatePriceAsync(string productId, decimal newPrice)
    {
        // Write-through: delegate first, then invalidate the stale cache entry.
        await inner.UpdatePriceAsync(productId, newPrice);
        if (_productCache.Remove(productId))
            Log.Information("  [CachingProxy] Cache invalidated for product {Id}", productId);
    }
}
