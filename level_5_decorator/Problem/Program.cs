// PROBLEM: Cross-cutting concerns (logging, validation, metrics) added via inheritance.
// With 3 concerns you need up to 7 subclasses to cover every combination:
//
//   BaseOrderProcessor
//   ├── LoggingProcessor
//   ├── ValidatingProcessor
//   ├── MetricsProcessor
//   ├── LoggingValidatingProcessor       (duplicates logging + validation delegation)
//   ├── LoggingMetricsProcessor          (duplicates logging + metrics delegation)
//   ├── ValidatingMetricsProcessor       (duplicates validation + metrics delegation)
//   └── LoggingValidatingMetricsProcessor (duplicates all three)
//
// None of the subclasses are reusable independently across different processor types.
// Every new processor type (DigitalOrderProcessor, PhysicalOrderProcessor) multiplies
// the class count again.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: Subclass explosion for cross-cutting concerns ===");
Log.Information("");

var order = new Order("ORD-001", "CUST-001", 149.99m);

Log.Information("--- Using LoggingValidatingMetricsProcessor (deepest subclass) ---");
var processor = new LoggingValidatingMetricsProcessor();
await processor.ProcessAsync(order);

Log.Information("");
Log.Information("--- What if we want Logging + Metrics but NOT Validation? ---");
Log.Information("  Need a separate LoggingMetricsProcessor subclass.");
Log.Information("  Every new combination = a new class. None are composable.");
Log.Information("");
Log.Information("--- Adding a 4th concern (fraud check) requires: ---");
Log.Information("  - FraudProcessor");
Log.Information("  - LoggingFraudProcessor");
Log.Information("  - ValidatingFraudProcessor  ...(8 more combinations)");
Log.Information("  2^4 = 16 subclasses for 4 concerns. Combinatorial explosion.");
Log.Information("");
Log.Information(">>> Fix: Decorator — each concern is one class, composable in any order.");

// ─── Domain ───────────────────────────────────────────────────────────────────

public record Order(string OrderId, string CustomerId, decimal Amount);

public abstract class BaseOrderProcessor
{
    public virtual async Task ProcessAsync(Order order)
    {
        await Task.Delay(50); // simulate DB write
        Log.Information("  [Base] Order {Id} saved to database", order.OrderId);
    }
}

// Concern 1 — adds logging around the base call
public class LoggingProcessor : BaseOrderProcessor
{
    public override async Task ProcessAsync(Order order)
    {
        Log.Information("  [Logging] START processing {Id}", order.OrderId);
        await base.ProcessAsync(order);        // delegation boilerplate
        Log.Information("  [Logging] END processing {Id}", order.OrderId);
    }
}

// Concern 2 — validates before calling the base
public class ValidatingProcessor : BaseOrderProcessor
{
    public override async Task ProcessAsync(Order order)
    {
        if (order.Amount <= 0)
            throw new InvalidOperationException("Amount must be positive.");
        Log.Information("  [Validation] Order {Id} passed validation", order.OrderId);
        await base.ProcessAsync(order);        // delegation boilerplate (duplicated)
    }
}

// Combine concerns 1 + 2 — must duplicate delegation in a new subclass
public class LoggingValidatingProcessor : BaseOrderProcessor
{
    public override async Task ProcessAsync(Order order)
    {
        Log.Information("  [Logging] START processing {Id}", order.OrderId);   // ← copied
        if (order.Amount <= 0)
            throw new InvalidOperationException("Amount must be positive.");
        Log.Information("  [Validation] Order {Id} passed validation", order.OrderId); // ← copied
        await base.ProcessAsync(order);        // delegation boilerplate (duplicated again)
        Log.Information("  [Logging] END processing {Id}", order.OrderId);     // ← copied
    }
}

// Combine all three — yet more duplication
public class LoggingValidatingMetricsProcessor : BaseOrderProcessor
{
    public override async Task ProcessAsync(Order order)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Log.Information("  [Logging] START processing {Id}", order.OrderId);   // ← copied
        if (order.Amount <= 0)
            throw new InvalidOperationException("Amount must be positive.");
        Log.Information("  [Validation] Order {Id} passed validation", order.OrderId); // ← copied
        await base.ProcessAsync(order);        // delegation boilerplate (duplicated again)
        Log.Information("  [Logging] END processing {Id}", order.OrderId);     // ← copied
        Log.Information("  [Metrics] {Id} took {Ms}ms", order.OrderId, sw.ElapsedMilliseconds); // ← new
    }
}
