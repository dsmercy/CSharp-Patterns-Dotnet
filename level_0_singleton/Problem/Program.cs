// PROBLEM: Every class that needs a database connection calls new OrderDbContext().
// Run 10 parallel tasks and you get 10 separate connections opened simultaneously —
// exhausting the connection pool under load.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

// ─── Non-thread-safe naive "singleton" attempt ───────────────────────────────
// This is the ANTI-PATTERN: a plain null-check is not thread-safe.
// Under parallel load two threads can both see _instance == null and each
// create their own instance before either assigns the field.

Log.Information("=== PROBLEM: No Singleton — each task opens its own connection ===");
Log.Information("");

int connectionCount = 0;
var hashCodes = new System.Collections.Concurrent.ConcurrentBag<int>();

var tasks = Enumerable.Range(1, 10).Select(i => Task.Run(() =>
{
    // Every caller does this — creating a brand-new context every time.
    var ctx = new OrderDbContext();
    hashCodes.Add(ctx.GetHashCode());
    Interlocked.Increment(ref connectionCount);

    var order = new Order($"ORD-{i:D3}", $"CUST-{i:D3}", 99.99m * i);
    ctx.SaveOrder(order);
}));

await Task.WhenAll(tasks);

Log.Information("");
Log.Information("Total connections opened: {Count} (should be 1, but is {Count})", connectionCount);
Log.Information("Unique hash codes: {Unique} out of 10 tasks", hashCodes.Distinct().Count());
Log.Information("");
Log.Information(">>> Every task got a DIFFERENT instance — connection pool exhausted!");
Log.Information(">>> In production this causes System.InvalidOperationException:");
Log.Information(">>>   'Timeout expired. The timeout period elapsed prior to obtaining");
Log.Information(">>>    a connection from the pool.'");

// ─── Models ──────────────────────────────────────────────────────────────────

public class Order
{
    public string OrderId { get; }
    public string CustomerId { get; }
    public decimal Amount { get; }

    public Order(string orderId, string customerId, decimal amount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
    }
}

// BAD: no Singleton — every caller creates a new instance.
public class OrderDbContext
{
    private static int _openCount = 0;

    public OrderDbContext()
    {
        int n = Interlocked.Increment(ref _openCount);
        Log.Warning("  [Connection #{N}] Opening NEW database connection (hash: {Hash})", n, GetHashCode());
    }

    public void SaveOrder(Order order) =>
        Log.Information("  Saving order {OrderId} via connection hash {Hash}", order.OrderId, GetHashCode());
}
