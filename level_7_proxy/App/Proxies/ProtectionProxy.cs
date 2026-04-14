using Proxy.App.Models;
using Serilog;

namespace Proxy.App.Proxies;

// PATTERN CONCEPT: Protection Proxy (Access Control Proxy).
//
// Guards sensitive operations by checking the caller's role before delegating.
// Read operations are allowed for all roles; UpdatePriceAsync requires Admin.
//
// The real service contains no role-checking logic — that concern is fully
// encapsulated here, making it easy to test and change independently.
public class ProtectionProxy(IProductCatalogueService inner, string currentUserRole)
    : IProductCatalogueService
{
    public Task<Product> GetProductAsync(string productId)
    {
        Log.Information("  [ProtectionProxy] GET allowed for role '{Role}'", currentUserRole);
        return inner.GetProductAsync(productId);
    }

    public Task<IList<Product>> SearchAsync(string query, int page = 1)
    {
        Log.Information("  [ProtectionProxy] SEARCH allowed for role '{Role}'", currentUserRole);
        return inner.SearchAsync(query, page);
    }

    public Task UpdatePriceAsync(string productId, decimal newPrice)
    {
        // PATTERN CONCEPT: access control gate — delegate only if authorised.
        if (currentUserRole != "Admin")
        {
            Log.Warning("  [ProtectionProxy] UPDATE DENIED — role '{Role}' is not Admin", currentUserRole);
            throw new UnauthorizedAccessException(
                $"Role '{currentUserRole}' is not authorised to update prices. Required: Admin.");
        }

        Log.Information("  [ProtectionProxy] UPDATE allowed for role 'Admin'");
        return inner.UpdatePriceAsync(productId, newPrice);
    }
}
