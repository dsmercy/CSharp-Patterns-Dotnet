// PROBLEM: Every call site that creates an order duplicates its own switch/if block.
// Adding a new order type (BulkOrder) means updating EVERY call site.
// Miss one and the system silently creates the wrong order type.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: Scattered switch blocks — adding BulkOrder breaks silently ===");
Log.Information("");

// Simulated order requests coming from three different parts of the system.
var requests = new[]
{
    ("Digital",      "CUST-001", "eBook: C# in Depth",       29.99m),
    ("Physical",     "CUST-002", "Mechanical Keyboard",      149.99m),
    ("Subscription", "CUST-003", "Prime Monthly",             9.99m),
    ("Bulk",         "CUST-004", "Office Chairs x50",       4999.99m),
};

// ─── Call site 1: Checkout controller ────────────────────────────────────────
Log.Information("--- Call site 1: Checkout controller ---");
foreach (var (type, cust, product, amount) in requests)
{
    // BulkOrder added here — developer remembered this call site.
    var order = type switch
    {
        "Digital"      => (Order)new DigitalOrder(cust, product, amount),
        "Physical"     => new PhysicalOrder(cust, product, amount),
        "Subscription" => new SubscriptionOrder(cust, product, amount),
        "Bulk"         => new BulkOrder(cust, product, amount),   // ← added
        _              => throw new InvalidOperationException($"Unknown type: {type}")
    };
    Log.Information("  Checkout created: {Type} order {Id}", order.GetType().Name, order.OrderId);
}

Log.Information("");

// ─── Call site 2: Admin panel ─────────────────────────────────────────────────
Log.Information("--- Call site 2: Admin panel ---");
foreach (var (type, cust, product, amount) in requests)
{
    // BulkOrder added here too — developer found this one.
    var order = type switch
    {
        "Digital"      => (Order)new DigitalOrder(cust, product, amount),
        "Physical"     => new PhysicalOrder(cust, product, amount),
        "Subscription" => new SubscriptionOrder(cust, product, amount),
        "Bulk"         => new BulkOrder(cust, product, amount),   // ← added
        _              => throw new InvalidOperationException($"Unknown type: {type}")
    };
    Log.Information("  Admin created: {Type} order {Id}", order.GetType().Name, order.OrderId);
}

Log.Information("");

// ─── Call site 3: Nightly import job ─────────────────────────────────────────
Log.Information("--- Call site 3: Nightly import job (BulkOrder NOT added here!) ---");
foreach (var (type, cust, product, amount) in requests)
{
    try
    {
        // Developer forgot to update this call site.
        // BulkOrder silently falls to the default and throws — or worse, returns null.
        var order = type switch
        {
            "Digital"      => (Order)new DigitalOrder(cust, product, amount),
            "Physical"     => new PhysicalOrder(cust, product, amount),
            "Subscription" => new SubscriptionOrder(cust, product, amount),
            // "Bulk" ← MISSING — developer forgot this call site
            _              => throw new InvalidOperationException($"Unknown order type '{type}' — import job not updated!")
        };
        Log.Information("  Import created: {Type} order {Id}", order.GetType().Name, order.OrderId);
    }
    catch (InvalidOperationException ex)
    {
        Log.Error("  IMPORT FAILED: {Message}", ex.Message);
    }
}

Log.Information("");
Log.Information(">>> Fix: centralise creation in ONE factory class.");
Log.Information(">>> With Factory Method, adding BulkOrder means ONE new factory class,");
Log.Information(">>> not hunting down every switch block across the codebase.");

// ─── Domain types (inlined in Problem to keep it self-contained) ──────────────

public abstract class Order
{
    public string OrderId    { get; } = $"ORD-{Guid.NewGuid():N}"[..10].ToUpper();
    public string CustomerId { get; }
    public string Product    { get; }
    public decimal Amount    { get; }

    protected Order(string customerId, string product, decimal amount)
    {
        CustomerId = customerId;
        Product    = product;
        Amount     = amount;
    }
}

public class DigitalOrder(string c, string p, decimal a)      : Order(c, p, a) { }
public class PhysicalOrder(string c, string p, decimal a)     : Order(c, p, a) { }
public class SubscriptionOrder(string c, string p, decimal a) : Order(c, p, a) { }
public class BulkOrder(string c, string p, decimal a)         : Order(c, p, a) { }
