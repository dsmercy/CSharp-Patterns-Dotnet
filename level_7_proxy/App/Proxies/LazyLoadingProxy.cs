using Proxy.App.Models;
using Serilog;

namespace Proxy.App.Proxies;

// PATTERN CONCEPT: Lazy Loading Proxy (Virtual Proxy).
//
// The expensive RealProductCatalogueService constructor (200ms connection setup)
// is deferred until the first actual method call. If no product is ever requested
// in a given code path, the real service is never initialised.
//
// Lazy<T> provides thread-safe, one-time initialisation — the same mechanism
// used in the Singleton pattern (level 0).
public class LazyLoadingProxy(Func<IProductCatalogueService> factory)
    : IProductCatalogueService
{
    // PATTERN CONCEPT: Lazy<T> — real service created only on first access.
    private readonly Lazy<IProductCatalogueService> _inner = new(factory);

    public Task<Product> GetProductAsync(string productId)
    {
        Log.Information("  [LazyProxy] First access — real service initialised now (if not already)");
        return _inner.Value.GetProductAsync(productId);
    }

    public Task<IList<Product>> SearchAsync(string query, int page = 1)
        => _inner.Value.SearchAsync(query, page);

    public Task UpdatePriceAsync(string productId, decimal newPrice)
        => _inner.Value.UpdatePriceAsync(productId, newPrice);
}
