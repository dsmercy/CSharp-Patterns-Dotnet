// SOLUTION: Builder pattern — construct complex orders step-by-step with named,
// chainable methods. Validation lives in Build(), not scattered across call sites.

using Builder.App;
using Builder.App.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== Builder Pattern — Ecommerce Order Demo ===");
Log.Information("");

var director = new OrderDirector();

// ─── Order 1: Step-by-step manual build ──────────────────────────────────────
// PATTERN CONCEPT: every field is named — no positional guesswork, impossible to
// swap discount % and gift message, impossible to silently omit a required field.
Log.Information("--- Order 1: Manual build — physical order with discount and loyalty points ---");

var physicalOrder = new OrderBuilder()
    .WithCustomer("CUST-001")
    .WithItem("SKU-001", "Wireless Mouse",   2, 29.99m)
    .WithItem("SKU-002", "USB-C Hub",        1, 19.99m)
    .WithItem("SKU-003", "Laptop Stand",     1, 34.99m)
    .WithShippingAddress(new Address("12 High Street", "London", "EC1A 1BB", "UK"))
    .WithDiscount("SAVE10", 10m)
    .WithDeliveryWindow("9am–12pm")
    .WithPaymentMethod("CreditCard")
    .WithLoyaltyPoints(150)
    .Build();

PrintOrder(physicalOrder);

// ─── Order 2: Director builds a gift order ────────────────────────────────────
// PATTERN CONCEPT: the director encodes the standard gift-order sequence.
// The caller only needs to add the product being gifted — the rest is canonical.
Log.Information("--- Order 2: Director builds a gift order ---");

var giftBuilder = new OrderBuilder();
director.BuildGiftOrder(giftBuilder, "CUST-002", "Happy Birthday! 🎂");

// Caller adds the actual gift product on top of what the director set up.
var giftOrder = giftBuilder
    .WithItem("SKU-BOOK", "The Pragmatic Programmer", 1, 39.99m)
    .WithShippingAddress(new Address("5 Baker Street", "London", "NW1 6XE", "UK"))
    .Build();

PrintOrder(giftOrder);

// ─── Order 3: Director builds a subscription order ───────────────────────────
Log.Information("--- Order 3: Director builds a subscription order ---");

var subBuilder = new OrderBuilder();
director.BuildSubscriptionOrder(subBuilder, "CUST-003", "Premium Monthly Plan", 14.99m);

var subOrder = subBuilder.Build();

PrintOrder(subOrder);

// ─── Order 4: Validation in action — Build() catches the missing item ─────────
Log.Information("--- Order 4: Build() catches missing items at construction time ---");
try
{
    var invalid = new OrderBuilder()
        .WithCustomer("CUST-004")
        // No WithItem() call — Build() rejects this immediately
        .Build();
}
catch (InvalidOperationException ex)
{
    Log.Error("  BUILD REJECTED: {Message}", ex.Message);
    Log.Information("  >>> Caught at construction, not at dispatch time — no silent failures.");
}

Log.Information("");
Log.Information("=== Key point ===");
Log.Information("  Adding an 11th field (e.g. WithLoyaltyPoints) adds ONE method to OrderBuilder.");
Log.Information("  All existing call sites compile unchanged — non-breaking by design.");

// ─── Helper ───────────────────────────────────────────────────────────────────

static void PrintOrder(Order o)
{
    Log.Information("  Order      : {Id}", o.OrderId);
    Log.Information("  Customer   : {C}", o.CustomerId);
    Log.Information("  Items      : {N}", o.Items.Count);
    foreach (var item in o.Items)
        Log.Information("               {Name} x{Qty} @ £{Price:F2} = £{Total:F2}",
            item.Name, item.Quantity, item.UnitPrice, item.LineTotal);
    Log.Information("  Discount   : {Code} {Pct}%", o.DiscountCode ?? "—", o.DiscountPercent);
    Log.Information("  Total      : £{Amount:F2}", o.Amount);
    Log.Information("  Shipping   : {Addr}", o.ShippingAddress?.ToString() ?? "—");
    Log.Information("  Gift msg   : {Msg}", o.GiftMessage ?? "—");
    Log.Information("  Delivery   : {W}", o.DeliveryWindow ?? "—");
    Log.Information("  Payment    : {P}", o.PaymentMethod);
    Log.Information("  Recurring  : {R}", o.IsRecurring);
    Log.Information("  Loyalty pts: {L}", o.LoyaltyPoints);
    Log.Information("");
}
