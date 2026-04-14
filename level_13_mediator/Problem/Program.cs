// PROBLEM: Components hold direct references to each other — a tangled web.
//
// Call chain: BadPaymentComponent → BadInventoryComponent
//                                 → BadShippingComponent
//                                   → BadNotificationComponent
//
// Two pain points demonstrated:
//   1. Adding LoyaltyComponent after payment requires editing BadPaymentComponent
//      to add a new constructor parameter and a new method call. ← MUST EDIT
//   2. If BadNotificationComponent ever needs to call back into BadPaymentComponent
//      (e.g. to verify a refund), a circular dependency forms — impossible to
//      construct without lazy initialisation hacks.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: Components wired directly to each other ===");
Log.Information("");

// Wiring the chain requires knowing the entire call graph up front.
var notification = new BadNotificationComponent();
var shipping     = new BadShippingComponent(notification);
var inventory    = new BadInventoryComponent(shipping);
// PROBLEM: BadPaymentComponent must be updated to accept LoyaltyComponent too. ← MUST EDIT
var payment      = new BadPaymentComponent(inventory);

var order = new Order("ORD-001", "CUST-42", 149.99m, "Laptop Stand");

Log.Information("--- Checkout ---");
await payment.ProcessPaymentAsync(order);
Log.Information("");

Log.Information(">>> To add LoyaltyComponent after payment:");
Log.Information("     1. Create LoyaltyComponent class");
Log.Information("     2. Add 'private readonly LoyaltyComponent _loyalty' to BadPaymentComponent  ← EDIT");
Log.Information("     3. Add loyalty ctor parameter to BadPaymentComponent                        ← EDIT");
Log.Information("     4. Call '_loyalty.AwardPointsAsync(order)' inside ProcessPaymentAsync       ← EDIT");
Log.Information(">>> Also: if NotificationComponent needs to verify refund with PaymentComponent,");
Log.Information(">>>   a circular dependency forms — Payment → Notification → Payment.");
Log.Information(">>> Fix: Mediator — components only know the mediator, never each other.");

// ─── Domain ───────────────────────────────────────────────────────────────────

public record Order(string OrderId, string CustomerId, decimal Amount, string Product);

// PROBLEM: each component is tightly coupled to the next in the chain.
public class BadPaymentComponent(BadInventoryComponent inventory)
// ↑ ADD LoyaltyComponent parameter here when requirement arrives  ← MUST EDIT
{
    public async Task ProcessPaymentAsync(Order order)
    {
        await Task.Delay(50);
        Log.Information("  [BadPayment]      Charged £{Amount:F2} for order {Id}", order.Amount, order.OrderId);

        // Direct call — BadPaymentComponent KNOWS about BadInventoryComponent.
        await inventory.ReserveStockAsync(order);
        // ↓ _loyalty.AwardPointsAsync(order) would be added HERE  ← MUST EDIT
    }
}

public class BadInventoryComponent(BadShippingComponent shipping)
{
    public async Task ReserveStockAsync(Order order)
    {
        await Task.Delay(30);
        Log.Information("  [BadInventory]    Stock reserved for '{Product}'", order.Product);

        // Direct call — BadInventoryComponent KNOWS about BadShippingComponent.
        await shipping.BookShipmentAsync(order);
    }
}

public class BadShippingComponent(BadNotificationComponent notification)
{
    public async Task BookShipmentAsync(Order order)
    {
        await Task.Delay(40);
        Log.Information("  [BadShipping]     Shipment booked for order {Id}", order.OrderId);

        // Direct call — BadShippingComponent KNOWS about BadNotificationComponent.
        await notification.SendConfirmationAsync(order);
    }
}

public class BadNotificationComponent
{
    // PROBLEM: if this class needs to call back into BadPaymentComponent,
    // a circular constructor dependency forms.
    public async Task SendConfirmationAsync(Order order)
    {
        await Task.Delay(20);
        Log.Information("  [BadNotification] Confirmation sent to customer {Id}", order.CustomerId);
    }
}
