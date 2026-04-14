using Proxy.App.Models;

namespace Proxy.App;

// PATTERN CONCEPT: the Subject interface.
// Both RealProductCatalogueService and every Proxy implement this.
// Callers depend only on IProductCatalogueService — they never know whether
// they are talking to the real service or a proxy in front of it.
public interface IProductCatalogueService
{
    Task<Product>        GetProductAsync(string productId);
    Task<IList<Product>> SearchAsync(string query, int page = 1);
    Task                 UpdatePriceAsync(string productId, decimal newPrice); // Admin only
}
