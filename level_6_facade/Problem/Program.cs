// PROBLEM: Every call site that places an order must orchestrate all 5 subsystems
// in exactly the right sequence. Two problems demonstrated:
//   1. The same 20-line orchestration block is duplicated across web and mobile checkouts.
//   2. Mobile checkout silently diverges — it skips the loyalty step.
//      This is easy to miss in code review and impossible to enforce centrally.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: Checkout orchestration duplicated across call sites ===");
Log.Information("");

var inventory    = new InventoryService();
var payment      = new PaymentService();
var shipping     = new ShippingService();
var notification = new NotificationService();
var loyalty      = new LoyaltyService();

var cart = new Cart("CUST-001", 149.99m,
    [new CartItem("SKU-001", "Wireless Mouse", 2, 29.99m),
     new CartItem("SKU-002", "USB-C Hub",      1, 89.99m + 0.02m)],
    "12 High Street, London EC1A 1BB");

// ─── Call site 1: Web checkout ────────────────────────────────────────────────
Log.Information("--- Call site 1: Web checkout (orchestrates all 5 subsystems) ---");
{
    // Step 1 — reserve stock
    await inventory.ReserveAsync(cart.Items);

    // Step 2 — charge payment
    var txnId = await payment.ChargeAsync(cart.CustomerId, cart.Total);

    // Step 3 — book shipping
    var trackingNum = await shipping.BookAsync(cart.ShippingAddress, cart.Items);

    // Step 4 — send confirmation
    await notification.SendConfirmationAsync(cart.CustomerId, txnId, trackingNum);

    // Step 5 — award loyalty points
    await loyalty.AwardPointsAsync(cart.CustomerId, cart.Total);     // ← present in web

    Log.Information("  [WebCheckout] Done — txn: {Txn}, tracking: {Track}", txnId, trackingNum);
}

Log.Information("");

// ─── Call site 2: Mobile checkout ─────────────────────────────────────────────
// PROBLEM: identical orchestration duplicated. And loyalty was forgotten.
Log.Information("--- Call site 2: Mobile checkout (loyalty step silently missing!) ---");
{
    // Step 1 — reserve stock  (duplicated)
    await inventory.ReserveAsync(cart.Items);

    // Step 2 — charge payment  (duplicated)
    var txnId = await payment.ChargeAsync(cart.CustomerId, cart.Total);

    // Step 3 — book shipping  (duplicated)
    var trackingNum = await shipping.BookAsync(cart.ShippingAddress, cart.Items);

    // Step 4 — send confirmation  (duplicated)
    await notification.SendConfirmationAsync(cart.CustomerId, txnId, trackingNum);

    // Step 5 ← MISSING: loyalty.AwardPointsAsync — developer forgot it here!
    // No compiler warning. Mobile customers silently miss their loyalty points.

    Log.Information("  [MobileCheckout] Done — txn: {Txn}, tracking: {Track}", txnId, trackingNum);
}

Log.Information("");
Log.Information(">>> Adding a 6th step (fraud check) requires editing BOTH call sites.");
Log.Information(">>> Miss one and fraud runs on web but not mobile. Silent divergence.");
Log.Information(">>> Fix: CheckoutFacade.PlaceOrderAsync(cart) — one canonical sequence.");

// ─── Subsystems (inline for self-contained demo) ──────────────────────────────

public record CartItem(string Sku, string Name, int Qty, decimal UnitPrice);
public record Cart(string CustomerId, decimal Total, List<CartItem> Items, string ShippingAddress);

public class InventoryService
{
    public Task ReserveAsync(List<CartItem> items)
    {
        foreach (var item in items)
            Log.Information("  [Inventory] Reserved {Qty}x {Name}", item.Qty, item.Name);
        return Task.CompletedTask;
    }
}

public class PaymentService
{
    public Task<string> ChargeAsync(string customerId, decimal amount)
    {
        var txn = $"TXN-{Guid.NewGuid():N}"[..14].ToUpper();
        Log.Information("  [Payment] Charged £{Amount:F2} for {CustomerId} — txn: {Txn}",
            amount, customerId, txn);
        return Task.FromResult(txn);
    }
}

public class ShippingService
{
    public Task<string> BookAsync(string address, List<CartItem> items)
    {
        var tracking = $"TRK-{Guid.NewGuid():N}"[..12].ToUpper();
        Log.Information("  [Shipping] Booked {ItemCount} item(s) to {Address} — tracking: {Track}",
            items.Count, address[..Math.Min(20, address.Length)], tracking);
        return Task.FromResult(tracking);
    }
}

public class NotificationService
{
    public Task SendConfirmationAsync(string customerId, string txnId, string trackingNum)
    {
        Log.Information("  [Notification] Confirmation email sent to {CustomerId}", customerId);
        return Task.CompletedTask;
    }
}

public class LoyaltyService
{
    public Task AwardPointsAsync(string customerId, decimal amount)
    {
        var points = (int)(amount * 10);
        Log.Information("  [Loyalty] Awarded {Points} points to {CustomerId}", points, customerId);
        return Task.CompletedTask;
    }
}
