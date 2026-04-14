// SOLUTION: Singleton pattern — OrderDbContext and ConfigurationService are each
// initialised exactly once, no matter how many tasks access them in parallel.

using Serilog;
using Singleton.App;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

// ─── Read config (parsed once) ────────────────────────────────────────────────
var config = ConfigurationService.Instance;
var appName = config.GetValue("App:Name", "Ecommerce Demo");
var maxOrders = int.Parse(config.GetValue("App:MaxParallelOrders", "10"));

Log.Information("=== {AppName} — Singleton Pattern Demo ===", appName);
Log.Information("");

// ─── Part 1: Prove one shared DB connection across 10 parallel tasks ──────────
Log.Information("--- Part 1: {N} parallel tasks sharing ONE OrderDbContext ---", maxOrders);
Log.Information("");

var hashCodes = new System.Collections.Concurrent.ConcurrentBag<int>();

var tasks = Enumerable.Range(1, maxOrders).Select(i => Task.Run(() =>
{
    // Every task goes through Instance — Lazy<T> ensures only one object exists.
    var ctx = OrderDbContext.Instance;
    hashCodes.Add(ctx.GetHashCode());

    var order = new Order($"ORD-{i:D3}", $"CUST-{i:D3}", 9.99m * i);
    ctx.SaveOrder(order);
}));

await Task.WhenAll(tasks);

Log.Information("");
Log.Information("Unique hash codes seen across {N} tasks: {Unique}", maxOrders, hashCodes.Distinct().Count());
Log.Information("ReferenceEquals check — same instance: {Same}",
    ReferenceEquals(OrderDbContext.Instance, OrderDbContext.Instance));

// ─── Part 2: Config singleton also initialised once ───────────────────────────
Log.Information("");
Log.Information("--- Part 2: ConfigurationService accessed from multiple call sites ---");
Log.Information("");

// Simulate three different services each calling Instance independently.
var cfg1 = ConfigurationService.Instance;
var cfg2 = ConfigurationService.Instance;
var cfg3 = ConfigurationService.Instance;

Log.Information("cfg1 hash: {H1} | cfg2 hash: {H2} | cfg3 hash: {H3}",
    cfg1.GetHashCode(), cfg2.GetHashCode(), cfg3.GetHashCode());
Log.Information("All the same instance: {Same}", cfg1.GetHashCode() == cfg2.GetHashCode()
                                                && cfg2.GetHashCode() == cfg3.GetHashCode());

// ─── Part 3: Query using the same shared connection ───────────────────────────
Log.Information("");
Log.Information("--- Part 3: Find order using the shared connection ---");
Log.Information("");

var found = OrderDbContext.Instance.FindOrder("ORD-005");
if (found is not null)
    Log.Information("Found: {OrderId} for customer {CustomerId}", found.OrderId, found.CustomerId);

Log.Information("");
Log.Information("=== Summary ===");
Log.Information("  OrderDbContext  — created ONCE, reused {N} times", maxOrders);
Log.Information("  ConfigService   — JSON parsed ONCE, read 3 times");
Log.Information("  Connection pool — never exhausted (1 connection, not {N})", maxOrders);
