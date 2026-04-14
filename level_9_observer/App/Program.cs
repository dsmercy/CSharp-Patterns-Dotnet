// SOLUTION: Observer pattern — OrderService broadcasts events to any number of
// registered observers. Adding LoyaltyObserver is a new class + one Subscribe()
// call. OrderService is never touched.
//
// Teaching points demonstrated:
//   1. Dynamic subscribe / unsubscribe at runtime
//   2. Per-observer exception isolation — SMTP failure does NOT abort inventory update
//   3. Two event types handled by the same observers
//   4. Language note: C# `event`/`EventHandler<T>` is the built-in shortcut
//      for this pattern; IOrderObserver is the explicit hand-rolled version
//      which makes the mechanics fully visible.

using Observer.App;
using Observer.App.Models;
using Observer.App.Observers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== SOLUTION: Observer pattern ===");
Log.Information("");

var orderService = new OrderService();

var inventory  = new InventoryObserver();
// simulateFailure: true — SMTP down on cancellations; proves isolation
var notify     = new NotificationObserver(simulateFailure: true);
var analytics  = new AnalyticsObserver();

// ── Section 1: Register all three observers ───────────────────────────────────
Log.Information("--- Registering observers ---");
orderService.Subscribe(inventory);
orderService.Subscribe(notify);
orderService.Subscribe(analytics);
Log.Information("");

// ── Section 2: Place an order ─────────────────────────────────────────────────
Log.Information("--- Placing order ORD-001 ---");
var order1 = new Order("ORD-001", "CUST-42", 149.99m, ["Laptop Stand", "USB-C Hub"]);
await orderService.PlaceOrderAsync(order1);
Log.Information("");

// ── Section 3: Cancel an order — NotificationObserver throws, others continue ─
Log.Information("--- Cancelling order ORD-001 (NotificationObserver will throw) ---");
await orderService.CancelOrderAsync(order1);
Log.Information("");

// ── Section 4: Unsubscribe AnalyticsObserver, place another order ─────────────
Log.Information("--- Unsubscribing AnalyticsObserver ---");
orderService.Unsubscribe(analytics);
Log.Information("");

Log.Information("--- Placing order ORD-002 (only Inventory + Notification notified) ---");
var order2 = new Order("ORD-002", "CUST-99", 89.00m, ["Mechanical Keyboard"]);
await orderService.PlaceOrderAsync(order2);
Log.Information("");

// ── Note on C# event keyword ──────────────────────────────────────────────────
Log.Information(">>> Language note: C# 'event EventHandler<T>' is the runtime-built-in");
Log.Information(">>>   version of this pattern. IOrderObserver makes the mechanics");
Log.Information(">>>   explicit. MediatR INotificationHandler<T> is the DI-friendly");
Log.Information(">>>   real-world equivalent used in production CQRS stacks.");
