// SOLUTION: Facade pattern — every call site calls CheckoutFacade.PlaceOrderAsync(cart).
// The 5-step orchestration lives in ONE place; call sites are 1 line each.

using Facade.App.Facade;
using Facade.App.Models;
using Facade.App.Subsystems;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

// Subsystems wired once (in a real ASP.NET Core app these are DI registrations).
var inventory    = new InventoryService();
var payment      = new PaymentService();
var shipping     = new ShippingService();
var notification = new NotificationService();
var loyalty      = new LoyaltyService();

// PATTERN CONCEPT: the Facade is the only dependency any call site needs.
var checkout = new CheckoutFacade(inventory, payment, shipping, notification, loyalty);

var cart = new Cart(
    CustomerId:      "CUST-001",
    Items:           [
        new CartItem("SKU-001", "Wireless Mouse",  2, 29.99m),
        new CartItem("SKU-002", "USB-C Hub",       1, 89.99m),
        new CartItem("SKU-003", "Laptop Stand",    1, 34.99m),
    ],
    ShippingAddress: new Address("12 High Street", "London", "EC1A 1BB"));

Log.Information("=== Facade Pattern — Checkout Demo ===");
Log.Information("");

// ─── Call site 1: Web checkout — one line ─────────────────────────────────────
Log.Information("--- Call site 1: Web checkout (one line) ---");
var result1 = await checkout.PlaceOrderAsync(cart);
PrintResult(result1);

Log.Information("");

// ─── Call site 2: Mobile checkout — identical one line ────────────────────────
// PATTERN CONCEPT: loyalty step cannot be forgotten — it is inside the Facade.
Log.Information("--- Call site 2: Mobile checkout (same one line, loyalty included) ---");
var mobileCart = cart with { CustomerId = "CUST-002" };
var result2    = await checkout.PlaceOrderAsync(mobileCart);
PrintResult(result2);

Log.Information("");

// ─── Facade is TRANSPARENT — direct subsystem access still works ──────────────
// PATTERN CONCEPT: the Facade does not lock callers out of subsystems.
// An admin tool that only needs to release a reservation can still call
// InventoryService.ReleaseAsync directly without going through PlaceOrderAsync.
Log.Information("--- Direct subsystem access (admin releasing a reservation) ---");
await inventory.ReleaseAsync(cart.Items);
Log.Information("");

Log.Information("=== Key point ===");
Log.Information("  Web, mobile, and admin call sites: 1 line each.");
Log.Information("  Adding a 6th step (FraudService) = one edit inside CheckoutFacade.");
Log.Information("  All call sites pick it up automatically.");

static void PrintResult(CheckoutResult r) =>
    Log.Information("  Result — txn: {Txn} | tracking: {Track} | loyalty pts: {Pts}",
        r.TransactionId, r.TrackingNumber, r.LoyaltyPointsAwarded);
