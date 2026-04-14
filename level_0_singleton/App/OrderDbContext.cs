using Serilog;

namespace Singleton.App;

// PATTERN CONCEPT: Lazy<T> guarantees thread-safe, one-time initialisation
// without explicit locking. The CLR ensures the factory lambda runs exactly once,
// even when multiple threads race to access .Value simultaneously.
//
// Why sealed?  Subclassing could introduce a public constructor, allowing callers
// to bypass the private constructor and create additional instances.
public sealed class OrderDbContext
{
    // Lazy<T> with default LazyThreadSafetyMode.ExecutionAndPublication — the
    // factory runs once; all other threads block until the first completes.
    private static readonly Lazy<OrderDbContext> _instance =
        new(() => new OrderDbContext());

    // The single global access point.
    public static OrderDbContext Instance => _instance.Value;

    // Private constructor — no external code can call new OrderDbContext().
    private OrderDbContext()
    {
        // In a real app this is where you'd open the connection / create the pool.
        Log.Information("OrderDbContext initialised — database connection opened (hash: {Hash})", GetHashCode());
    }

    public void SaveOrder(Order order) =>
        Log.Information("  Saving order {OrderId} (£{Amount:F2}) via connection hash {Hash}",
            order.OrderId, order.Amount, GetHashCode());

    public Order? FindOrder(string orderId)
    {
        Log.Information("  Querying order {OrderId} via connection hash {Hash}", orderId, GetHashCode());
        // Simulated result — no real DB in this demo.
        return new Order(orderId, "CUST-001", 49.99m);
    }
}
