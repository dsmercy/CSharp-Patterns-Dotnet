// SOLUTION: Mediator pattern — components communicate only through CheckoutMediator.
//
// Teaching points demonstrated:
//   1. Successful checkout — payment → inventory → shipping → notification,
//      each step triggered by the mediator, never by colleagues directly.
//   2. Failed payment — mediator routes PaymentFailed to notification only;
//      inventory and shipping are never involved.
//   3. Refund — payment signals PaymentRefunded; mediator releases stock AND
//      sends refund email in parallel with a single case block.
//   4. Adding LoyaltyComponent: new class + one new case in NotifyAsync.
//      Zero changes to any existing colleague.
//
// Mediator vs Observer distinction:
//   Observer — subject broadcasts; observers react independently, no sequencing.
//   Mediator — mediator controls sequence: PaymentSucceeded → THEN reserve stock
//              → THEN book shipping. Order matters; the mediator enforces it.

using Mediator.App;
using Mediator.App.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== SOLUTION: Mediator pattern ===");
Log.Information("");

var mediator = new CheckoutMediator();

// ── Scenario 1: Successful checkout ──────────────────────────────────────────
Log.Information("--- Scenario 1: successful checkout ---");
var order1 = new Order("ORD-001", "CUST-42", 149.99m, "Laptop Stand");
await mediator.CheckoutAsync(order1);
Log.Information("");

// ── Scenario 2: Payment failure ───────────────────────────────────────────────
Log.Information("--- Scenario 2: payment failure — inventory and shipping never called ---");
var order2 = new Order("ORD-002", "CUST-99", 89.00m, "USB-C Hub");
await mediator.CheckoutFailedAsync(order2);
Log.Information("");

// ── Scenario 3: Refund ────────────────────────────────────────────────────────
Log.Information("--- Scenario 3: refund — stock released + refund email sent ---");
var order3 = new Order("ORD-003", "CUST-07", 250.00m, "Mechanical Keyboard");
await mediator.RefundAsync(order3);
Log.Information("");

// ── Notes ─────────────────────────────────────────────────────────────────────
Log.Information(">>> PaymentComponent has zero imports of Inventory/Shipping/Notification.");
Log.Information(">>> To add LoyaltyComponent after payment:");
Log.Information(">>>   1. Create LoyaltyComponent : CheckoutColleague");
Log.Information(">>>   2. Add case CheckoutEvent.PaymentSucceeded: call loyalty THEN inventory");
Log.Information(">>>   Zero existing colleague classes change.");
Log.Information(">>> Real-world: MediatR IMediator.Send() is this pattern as a library.");
Log.Information(">>> Keep mediators focused — one workflow per mediator to avoid god object.");
