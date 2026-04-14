// PROBLEM: BadOrderValidator contains four levels of nested if/else blocks —
// one for stock, payment, fraud, and address.
//
// Two pain points demonstrated:
//   1. Adding a "region restriction" check at position 2 in the chain requires
//      restructuring 3 other if blocks and re-indenting their bodies.
//      Every line that must change is marked ← RESTRUCTURE.
//   2. The failure reason is buried deep inside conditional nesting — callers
//      cannot distinguish *why* validation failed without string parsing.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: BadOrderValidator — nested if/else blocks ===");
Log.Information("");

var validator = new BadOrderValidator();

var orders = new[]
{
    new Order("ORD-001", "CUST-01", 500m,   "GB", HasValidPayment: true,  IsFraudulent: false),
    new Order("ORD-002", "CUST-02", 15000m, "GB", HasValidPayment: true,  IsFraudulent: false), // stock fail
    new Order("ORD-003", "CUST-03", 200m,   "GB", HasValidPayment: false, IsFraudulent: false), // payment fail
    new Order("ORD-004", "CUST-04", 300m,   "GB", HasValidPayment: true,  IsFraudulent: true),  // fraud fail
    new Order("ORD-005", "CUST-05", 400m,   "XX", HasValidPayment: true,  IsFraudulent: false), // address fail
};

foreach (var order in orders)
{
    var (passed, reason) = validator.Validate(order);
    if (passed)
        Log.Information("  {Id} — PASSED", order.OrderId);
    else
        Log.Error("  {Id} — FAILED: {Reason}", order.OrderId, reason);
}

Log.Information("");
Log.Information(">>> To insert a RegionRestriction check at position 2:");
Log.Information("     1. Wrap existing payment block in a new if/else  ← RESTRUCTURE");
Log.Information("     2. Re-indent fraud block inside that             ← RESTRUCTURE");
Log.Information("     3. Re-indent address block inside that           ← RESTRUCTURE");
Log.Information("     4. Total: 3 existing blocks restructured for 1 new check.");
Log.Information(">>> Fix: Chain of Responsibility — each check is its own class.");

// ─── Domain ───────────────────────────────────────────────────────────────────

public record Order(
    string OrderId,
    string CustomerId,
    decimal Amount,
    string ShippingRegion,
    bool HasValidPayment,
    bool IsFraudulent);

// PROBLEM: every new check reshapes this method.
public class BadOrderValidator
{
    private static readonly HashSet<string> _allowedRegions = ["GB", "US", "EU"];

    public (bool Passed, string Reason) Validate(Order order)
    {
        // Level 1 — stock check
        if (order.Amount > 10_000)
        {
            return (false, "STOCK_UNAVAILABLE: Quantity exceeds available stock.");
        }
        else
        {
            // Level 2 — payment check  ← RESTRUCTURE if region check inserted here
            if (!order.HasValidPayment)
            {
                return (false, "PAYMENT_INVALID: Payment credentials failed validation.");
            }
            else
            {
                // Level 3 — fraud check  ← RESTRUCTURE (re-indent entire block)
                if (order.IsFraudulent)
                {
                    return (false, "FRAUD_DETECTED: Order flagged by fraud detection.");
                }
                else
                {
                    // Level 4 — address check  ← RESTRUCTURE (re-indent entire block)
                    if (!_allowedRegions.Contains(order.ShippingRegion))
                    {
                        return (false, $"ADDRESS_INVALID: Region '{order.ShippingRegion}' is not supported.");
                    }
                    else
                    {
                        return (true, string.Empty);
                    }
                }
            }
        }
    }
}
