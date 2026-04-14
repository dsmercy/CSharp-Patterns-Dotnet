// PROBLEM: Order has a 10-parameter constructor — most parameters are optional.
// Problems demonstrated:
//   1. Positional mistake — compilable but logically wrong (discount % and gift message swapped)
//   2. Missing required field — physical order without a shipping address throws at runtime
//   3. Adding an 11th field breaks EVERY existing call site (shown as comment diff)

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: 10-parameter constructor — unreadable, unsafe, fragile ===");
Log.Information("");

var items = new List<OrderItem>
{
    new("SKU-001", "Wireless Mouse", 2, 29.99m),
    new("SKU-002", "USB Hub",        1, 19.99m),
};

var shippingAddr = new Address("12 High Street", "London", "EC1A 1BB", "UK");

// ─── Order 1: Correct — but can you tell at a glance which arg is which? ──────
Log.Information("--- Order 1: Correct, but unreadable ---");
var order1 = new Order(
    "CUST-001",          // customerId     ← param 1
    items,               // items          ← param 2
    shippingAddr,        // shippingAddr   ← param 3
    null,                // billingAddr    ← param 4  (same as shipping — pass null)
    "SAVE10",            // discountCode   ← param 5
    10m,                 // discountPct    ← param 6
    null,                // giftMessage    ← param 7
    "9am-12pm",          // deliveryWindow ← param 8
    "CreditCard",        // paymentMethod  ← param 9
    false                // isRecurring    ← param 10
);
Log.Information("  Created order {Id} — total £{Total:F2}", order1.OrderId, order1.Total);

Log.Information("");

// ─── Order 2: Positional bug — discountPct and giftMessage swapped ─────────────
// The compiler accepts this. No warning. The order goes through with 0% discount
// and a gift message of "15" — only caught during QA or by the customer.
Log.Information("--- Order 2: Positional bug — discount % and gift message swapped ---");
var order2 = new Order(
    "CUST-002",
    items,
    shippingAddr,
    null,
    "GIFT15",
    "Happy Birthday!",  // ← BUG: this should be giftMessage (param 7), not discountPct (param 6)
    15m,                // ← BUG: this should be discountPct (param 6), not giftMessage (param 7)
    null,
    "CreditCard",
    false
);
// discountPct receives "Happy Birthday!" — decimal.Parse would throw in real code,
// but here we stored it as object so we show the logical corruption instead.
Log.Warning("  Order {Id}: discountPct = '{Pct}' | giftMessage = '{Msg}' — WRONG WAY ROUND!",
    order2.OrderId, order2.DiscountPct, order2.GiftMessage);

Log.Information("");

// ─── Order 3: Missing required field — shipping address is null ───────────────
Log.Information("--- Order 3: Missing shipping address — blows up at dispatch time ---");
try
{
    var order3 = new Order(
        "CUST-003",
        items,
        null,           // ← forgot shipping address — compiles fine, fails later
        null,
        null,
        0m,
        null,
        null,
        "DebitCard",
        false
    );

    // Somewhere downstream — could be minutes later in a background job:
    order3.Dispatch();
}
catch (InvalidOperationException ex)
{
    Log.Error("  DISPATCH FAILED: {Message}", ex.Message);
}

Log.Information("");

// ─── Adding an 11th field ─────────────────────────────────────────────────────
Log.Information("--- What happens when we add field 11: LoyaltyPoints? ---");
Log.Information("  Every single new Order(...) call site needs updating.");
Log.Information("  Miss one and it silently passes 0 loyalty points (wrong default).");
Log.Information("  With 10 positional args, finding the right slot is error-prone.");
Log.Information("");
Log.Information(">>> Fix: Builder pattern — each field is named, validated in one place,");
Log.Information(">>>      and adding a new field is non-breaking.");

// ─── Domain types ─────────────────────────────────────────────────────────────

public record OrderItem(string ProductId, string Name, int Quantity, decimal UnitPrice);

public record Address(string Street, string City, string PostCode, string Country);

public class Order
{
    public string    OrderId    { get; } = $"ORD-{Guid.NewGuid():N}"[..8].ToUpper();
    public string    CustomerId { get; }
    public List<OrderItem> Items { get; }

    // Storing as object only to demonstrate the positional-bug corruption visually.
    // In real code these would be typed and the compiler might catch the swap.
    public object?   DiscountPct     { get; }
    public object?   GiftMessage     { get; }

    public Address?  ShippingAddress { get; }
    public Address?  BillingAddress  { get; }
    public string?   DiscountCode    { get; }
    public string?   DeliveryWindow  { get; }
    public string?   PaymentMethod   { get; }
    public bool      IsRecurring     { get; }

    public decimal Total => Items.Sum(i => i.Quantity * i.UnitPrice);

    // 10-parameter constructor — the root of all pain
    public Order(
        string customerId,
        List<OrderItem> items,
        Address? shippingAddress,
        Address? billingAddress,
        string? discountCode,
        object? discountPct,       // object to allow the swap-bug demo
        object? giftMessage,       // object to allow the swap-bug demo
        string? deliveryWindow,
        string? paymentMethod,
        bool isRecurring)
    {
        CustomerId      = customerId;
        Items           = items;
        ShippingAddress = shippingAddress;
        BillingAddress  = billingAddress;
        DiscountCode    = discountCode;
        DiscountPct     = discountPct;
        GiftMessage     = giftMessage;
        DeliveryWindow  = deliveryWindow;
        PaymentMethod   = paymentMethod;
        IsRecurring     = isRecurring;
    }

    public void Dispatch()
    {
        if (ShippingAddress is null)
            throw new InvalidOperationException(
                $"Order {OrderId} cannot be dispatched — shipping address is missing.");

        Log.Information("  Dispatching order {Id} to {Street}, {City}", OrderId,
            ShippingAddress.Street, ShippingAddress.City);
    }
}
