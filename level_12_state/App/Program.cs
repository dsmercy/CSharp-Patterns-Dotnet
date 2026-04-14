// SOLUTION: State pattern — each lifecycle state is its own class.
//
// Teaching points demonstrated:
//   1. Happy path — Pending → Confirmed → Shipped → Delivered, zero conditionals
//      in the Order class itself.
//   2. Invalid transition on a Shipped order — throws immediately with a clear
//      message instead of silently doing nothing.
//   3. Cancel from Pending — valid; Cancel from Shipped — blocked.
//   4. Adding "ReturnRequested": one new ReturnRequestedState class + update
//      DeliveredState.Cancel() to transition instead of throwing.
//      Zero other state classes change.

using State.App;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== SOLUTION: State pattern ===");
Log.Information("");

// ── Scenario 1: Happy path ────────────────────────────────────────────────────
Log.Information("--- Scenario 1: happy path — full lifecycle ---");
var order1 = new Order("ORD-001", "CUST-42", 149.99m);
Log.Information("  Start: {Order}", order1);

order1.Confirm();
order1.Ship();
order1.Deliver();

Log.Information("  End:   {Order}", order1);
Log.Information("");

// ── Scenario 2: Cancel from Pending — allowed ─────────────────────────────────
Log.Information("--- Scenario 2: cancel from Pending — valid transition ---");
var order2 = new Order("ORD-002", "CUST-99", 89.00m);
Log.Information("  Start: {Order}", order2);
order2.Cancel();
Log.Information("  End:   {Order}", order2);
Log.Information("");

// ── Scenario 3: Cancel a Shipped order — blocked ──────────────────────────────
Log.Information("--- Scenario 3: cancel a Shipped order — state rejects it ---");
var order3 = new Order("ORD-003", "CUST-07", 250.00m);
order3.Confirm();
order3.Ship();
Log.Information("  State: {Order}", order3);
try
{
    order3.Cancel(); // ShippedState throws — not a silent no-op
}
catch (InvalidOperationException ex)
{
    Log.Error("  BLOCKED: {Message}", ex.Message);
}
Log.Information("  State after failed cancel: {Order}", order3);
Log.Information("");

// ── Scenario 4: Invalid transition on a Delivered order ───────────────────────
Log.Information("--- Scenario 4: cancel a Delivered order — DeliveredState throws ---");
var order4 = new Order("ORD-004", "CUST-11", 75.00m);
order4.Confirm();
order4.Ship();
order4.Deliver();
try
{
    order4.Cancel(); // Problem version silently did nothing — now throws
}
catch (InvalidOperationException ex)
{
    Log.Error("  BLOCKED: {Message}", ex.Message);
}
Log.Information("  State unchanged: {Order}", order4);
Log.Information("");

// ── Notes ─────────────────────────────────────────────────────────────────────
Log.Information(">>> Order class contains zero switch/if blocks about its own status.");
Log.Information(">>> Real-world: Task<T> states (Running/Faulted/Completed),");
Log.Information(">>>   Elsa/Stateless workflow engines, invoice/ticket lifecycles.");
Log.Information(">>> Adding 'ReturnRequested': new class + edit DeliveredState.Cancel().");
Log.Information(">>>   No other state class needs to change.");
