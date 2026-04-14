// PROBLEM: BadOrderService directly references InventoryService, EmailService,
// and AnalyticsService. Every new downstream action requires:
//   1. Opening BadOrderService and adding a field        ← MUST EDIT THIS CLASS
//   2. Adding a constructor parameter                    ← MUST EDIT THIS CLASS
//   3. Calling the new service inline                    ← MUST EDIT THIS CLASS
//
// Additionally, if EmailService throws the remaining calls (Analytics) are
// never reached — one failure aborts the entire notification chain.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: BadOrderService with hard-coded downstream calls ===");
Log.Information("");

var inventory  = new InventoryService();
var email      = new EmailService(simulateFailure: true); // SMTP down
var analytics  = new AnalyticsService();

// To add LoyaltyService: open BadOrderService, add field + ctor param + call. ← CHANGE
var svc = new BadOrderService(inventory, email, analytics);

Log.Information("--- Placing order ORD-001 ---");
await svc.PlaceOrderAsync(new Order("ORD-001", "CUST-42", 149.99m));
Log.Information("");

Log.Information(">>> To add LoyaltyService, BadOrderService MUST be edited:");
Log.Information("     1. Add 'private readonly LoyaltyService _loyalty'   ← ADD HERE");
Log.Information("     2. Add ctor parameter                                ← ADD HERE");
Log.Information("     3. Add '_loyalty.AwardPoints(order)' call inline    ← ADD HERE");
Log.Information(">>> Also: EmailService threw — AnalyticsService was never called.");
Log.Information(">>> Fix: Observer pattern — OrderService broadcasts; observers react.");

// ─── Domain ───────────────────────────────────────────────────────────────────

public record Order(string OrderId, string CustomerId, decimal Amount);

// PROBLEM: every new downstream action requires editing this class.
public class BadOrderService(
    InventoryService inventory,
    EmailService     email,
    AnalyticsService analytics)
    // ↓ ADD new service fields here when requirements grow  ←────────────────────
{
    public async Task PlaceOrderAsync(Order order)
    {
        Log.Information("  [BadOrderService] Placing order {Id}", order.OrderId);

        // Each call is hard-coded. Order matters, errors propagate upward.
        await inventory.ReserveStockAsync(order);

        try
        {
            await email.SendConfirmationAsync(order); // may throw
        }
        catch (Exception ex)
        {
            // Even with a catch here, the developer must remember to add one
            // for every new service — easy to forget.
            Log.Error("  [BadOrderService] Email failed: {Msg}", ex.Message);
            // AnalyticsService call below is still reached, but only because
            // the developer manually added this try/catch. Without it the
            // exception would bubble up and abort everything.
        }

        await analytics.RecordAsync(order);
        // ↓ New service call would be added HERE — requires opening this file ←──
    }
}

public class InventoryService
{
    public async Task ReserveStockAsync(Order order)
    {
        await Task.Delay(30);
        Log.Information("  [InventoryService] Stock reserved for order {Id}", order.OrderId);
    }
}

public class EmailService(bool simulateFailure = false)
{
    public async Task SendConfirmationAsync(Order order)
    {
        await Task.Delay(50);
        if (simulateFailure)
            throw new InvalidOperationException("SMTP connection refused.");
        Log.Information("  [EmailService] Confirmation sent for order {Id}", order.OrderId);
    }
}

public class AnalyticsService
{
    public async Task RecordAsync(Order order)
    {
        await Task.Delay(20);
        Log.Information("  [AnalyticsService] Event recorded for order {Id}", order.OrderId);
    }
}

// ── What LoyaltyService would look like (not wired up — forces BadOrderService edit) ──
public class LoyaltyService
{
    public async Task AwardPointsAsync(Order order)
    {
        await Task.Delay(20);
        Log.Information("  [LoyaltyService] Points awarded for order {Id}", order.OrderId);
    }
}
