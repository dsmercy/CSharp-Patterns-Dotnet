// SOLUTION: Proxy pattern — three proxy types demonstrated, then composed.

using Microsoft.Extensions.Configuration;
using Proxy.App;
using Proxy.App.Proxies;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var userRole       = config["Demo:UserRole"]        ?? "Guest";
var cacheTtlSec    = int.Parse(config["Demo:CacheTtlSeconds"] ?? "30");
var cacheTtl       = TimeSpan.FromSeconds(cacheTtlSec);

Log.Information("=== Proxy Pattern — Product Catalogue Demo ===");
Log.Information("  UserRole: {Role} | Cache TTL: {Ttl}s", userRole, cacheTtlSec);
Log.Information("");

// ─── Proxy 1: Caching Proxy ───────────────────────────────────────────────────
Log.Information("--- Proxy 1: Caching Proxy (5 requests for P001 → 1 API call) ---");
{
    IProductCatalogueService cachingProxy =
        new CachingProxy(new RealProductCatalogueService(), cacheTtl);

    Log.Information("");
    for (var i = 1; i <= 5; i++)
    {
        var p = await cachingProxy.GetProductAsync("P001");
        Log.Information("  Request {I}: '{Name}' £{Price:F2}", i, p.Name, p.Price);
    }

    Log.Information("");
    Log.Information("  Searching 'keyboard' (first call — miss, then repeat):");
    await cachingProxy.SearchAsync("keyboard");
    await cachingProxy.SearchAsync("keyboard");

    Log.Information("");
    Log.Information("  Updating price (write-through + cache invalidation):");
    await cachingProxy.UpdatePriceAsync("P001", 24.99m);

    Log.Information("");
    Log.Information("  Requesting P001 again after invalidation (should be a cache MISS):");
    var updated = await cachingProxy.GetProductAsync("P001");
    Log.Information("  Got '{Name}' £{Price:F2}", updated.Name, updated.Price);
}

Log.Information("");

// ─── Proxy 2: Lazy Loading Proxy ─────────────────────────────────────────────
Log.Information("--- Proxy 2: Lazy Loading Proxy (real service init deferred to first use) ---");
{
    // PATTERN CONCEPT: LazyLoadingProxy is created instantly — no heavy init yet.
    Log.Information("  Creating LazyLoadingProxy... (real service NOT initialised yet)");
    IProductCatalogueService lazyProxy =
        new LazyLoadingProxy(() => new RealProductCatalogueService());

    Log.Information("  Proxy created. Real service constructor has NOT run.");
    Log.Information("  Now making the first call — constructor fires here:");
    Log.Information("");

    var p = await lazyProxy.GetProductAsync("P002");
    Log.Information("  Got '{Name}' £{Price:F2}", p.Name, p.Price);

    Log.Information("");
    Log.Information("  Second call — real service already initialised, no repeat heavy init:");
    var p2 = await lazyProxy.GetProductAsync("P003");
    Log.Information("  Got '{Name}' £{Price:F2}", p2.Name, p2.Price);
}

Log.Information("");

// ─── Proxy 3: Protection Proxy ────────────────────────────────────────────────
Log.Information("--- Proxy 3: Protection Proxy (role from appsettings: {Role}) ---", userRole);
{
    IProductCatalogueService protectionProxy =
        new ProtectionProxy(new RealProductCatalogueService(), userRole);

    Log.Information("");
    Log.Information("  GET (allowed for all roles):");
    var p = await protectionProxy.GetProductAsync("P001");
    Log.Information("  Got '{Name}' £{Price:F2}", p.Name, p.Price);

    Log.Information("");
    Log.Information("  UpdatePrice (Admin only — current role: {Role}):", userRole);
    try
    {
        await protectionProxy.UpdatePriceAsync("P001", 19.99m);
        Log.Information("  Price updated successfully.");
    }
    catch (UnauthorizedAccessException ex)
    {
        Log.Error("  ACCESS DENIED: {Message}", ex.Message);
    }
}

Log.Information("");

// ─── Composed: Caching + Protection stacked ───────────────────────────────────
// PATTERN CONCEPT: proxies compose exactly like Decorators — each wraps the next.
// Outer = ProtectionProxy gates writes; Inner = CachingProxy avoids repeat reads.
Log.Information("--- Composed: Protection(Caching(Real)) ---");
Log.Information("  Write gate at the outside, cache close to the real service.");
{
    IProductCatalogueService composed =
        new ProtectionProxy(
            new CachingProxy(new RealProductCatalogueService(), cacheTtl),
            userRole);

    Log.Information("");
    await composed.GetProductAsync("P004");  // cache miss
    await composed.GetProductAsync("P004");  // cache hit

    Log.Information("");
    Log.Information("  Attempting UpdatePrice through composed proxy (role: {Role}):", userRole);
    try
    {
        await composed.UpdatePriceAsync("P004", 99.99m);
    }
    catch (UnauthorizedAccessException ex)
    {
        Log.Error("  ACCESS DENIED: {Message}", ex.Message);
    }
}

Log.Information("");
Log.Information("=== Key point ===");
Log.Information("  RealProductCatalogueService: zero caching, zero auth logic.");
Log.Information("  Each proxy adds one concern. Compose in any order needed.");
Log.Information("  Change appsettings.json Demo:UserRole to 'Admin' to allow UpdatePrice.");
