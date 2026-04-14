// PROBLEM: BadShippingCalculator contains a switch block for every strategy.
// Two problems demonstrated:
//   1. Adding "SameDay" shipping means opening BadShippingCalculator and editing it —
//      every line that must change is marked ← ADD/CHANGE HERE.
//   2. The "SameDay" case was not added to the switch yet — it throws at runtime,
//      silently falling through to the default.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: ShippingCalculator with embedded switch block ===");
Log.Information("");

var calculator = new BadShippingCalculator();
var order      = new Order("ORD-001", "CUST-001", 149.99m);

// Known strategies work fine today.
Log.Information("--- Known strategies ---");
foreach (var strategy in new[] { "Standard", "Express", "Free" })
{
    var (cost, days) = calculator.Calculate(order, strategy);
    Log.Information("  {Strategy,-10} → £{Cost:F2}, {Days} day(s)", strategy, cost, days);
}

Log.Information("");

// A new "SameDay" requirement arrives. Developer added it to the UI dropdown
// but forgot to update the switch in BadShippingCalculator.
Log.Information("--- New 'SameDay' strategy (not yet added to the switch) ---");
try
{
    calculator.Calculate(order, "SameDay");
}
catch (NotSupportedException ex)
{
    Log.Error("  RUNTIME FAILURE: {Message}", ex.Message);
}

Log.Information("");
Log.Information(">>> To add SameDay, BadShippingCalculator must be opened and edited:");
Log.Information("     1. Add 'case \"SameDay\":' to the switch        ← ADD HERE");
Log.Information("     2. Implement the cost formula inline            ← ADD HERE");
Log.Information("     3. Add the estimated-days branch               ← ADD HERE");
Log.Information("     4. Re-test all existing cases to check for regressions");
Log.Information(">>> Fix: Strategy pattern — add SameDayShippingStrategy class,");
Log.Information(">>>      zero changes to ShippingCalculator.");

// ─── Domain ───────────────────────────────────────────────────────────────────

public record Order(string OrderId, string CustomerId, decimal Amount);

public class BadShippingCalculator
{
    // PROBLEM: every new strategy requires editing this method.
    // Open for modification — violates Open/Closed Principle.
    public (decimal Cost, int Days) Calculate(Order order, string strategyName)
    {
        // ↓ ADD HERE when a new strategy arrives  ─────────────────────────────
        decimal cost = strategyName switch
        {
            "Standard" => 3.99m,
            "Express"  => order.Amount * 0.10m,
            "Free"     => order.Amount >= 50m ? 0m
                              : throw new InvalidOperationException("Need £50+ for free shipping."),
            // "SameDay" => ... ← developer forgot to add this
            _ => throw new NotSupportedException(
                     $"Unknown shipping strategy '{strategyName}'. Did you forget to add a case?")
        };

        // ↓ CHANGE HERE too — days live in a separate branch  ─────────────────
        int days = strategyName switch
        {
            "Standard" => 5,
            "Express"  => 1,
            "Free"     => 5,
            // "SameDay" => 0  ← also missing here
            _ => throw new NotSupportedException($"Unknown strategy '{strategyName}'")
        };

        return (cost, days);
    }
}
