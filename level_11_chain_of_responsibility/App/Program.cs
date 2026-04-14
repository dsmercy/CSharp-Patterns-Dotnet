// SOLUTION: Chain of Responsibility — each validator is its own class.
//
// Teaching points demonstrated:
//   1. Fluent chain assembly — stock.SetNext(payment).SetNext(fraud).SetNext(address)
//      The caller owns the order; validators know nothing about each other.
//   2. Chain stops at first failure — later handlers are never called.
//   3. Adding RegionRestrictionValidator: one new class + one SetNext() call.
//      No existing validator is touched. Open/Closed Principle preserved.
//   4. Cheap checks first — stock (10ms) runs before fraud (80ms).
//   5. Real-world parallel: ASP.NET Core Middleware pipeline.

using Chain.App.Models;
using Chain.App.Validators;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== SOLUTION: Chain of Responsibility ===");
Log.Information("");

// PATTERN CONCEPT: chain assembled by the caller — validators are decoupled.
var stock   = new StockAvailabilityValidator();
var payment = new PaymentValidator();
var fraud   = new FraudDetectionValidator();
var address = new AddressValidator();

stock.SetNext(payment).SetNext(fraud).SetNext(address);

// ── Test orders ───────────────────────────────────────────────────────────────

var orders = new[]
{
    // Passes all four checks
    new Order("ORD-001", "CUST-01", 500m,   "GB", HasValidPayment: true,  IsFraudulent: false),
    // Fails at stock (£15 000 > £10 000 limit) — payment/fraud/address never run
    new Order("ORD-002", "CUST-02", 15_000m,"GB", HasValidPayment: true,  IsFraudulent: false),
    // Fails at payment — fraud/address never run
    new Order("ORD-003", "CUST-03", 200m,   "GB", HasValidPayment: false, IsFraudulent: false),
    // Fails at fraud — address never run
    new Order("ORD-004", "CUST-04", 300m,   "GB", HasValidPayment: true,  IsFraudulent: true),
    // Fails at address — last handler
    new Order("ORD-005", "CUST-05", 400m,   "XX", HasValidPayment: true,  IsFraudulent: false),
};

foreach (var order in orders)
{
    Log.Information("--- Validating {Id} (£{Amount:F2}, region: {Region}) ---",
        order.OrderId, order.Amount, order.ShippingRegion);

    var result = await stock.ValidateAsync(order);

    if (result.Passed)
        Log.Information("  => {Id}: ALL CHECKS PASSED", order.OrderId);
    else
        Log.Error("  => {Id}: {Result}", order.OrderId, result);

    Log.Information("");
}

// ── Extending the chain ───────────────────────────────────────────────────────
Log.Information(">>> To add a RegionRestriction check between payment and fraud:");
Log.Information("     1. Create RegionRestrictionValidator : OrderValidator   (new class)");
Log.Information("     2. stock.SetNext(payment).SetNext(region).SetNext(fraud).SetNext(address)");
Log.Information("     3. Zero changes to existing validators.");
Log.Information(">>> Real-world: ASP.NET Core Middleware is this pattern — each");
Log.Information(">>>   middleware short-circuits or calls next().");
