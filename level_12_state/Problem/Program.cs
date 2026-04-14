// PROBLEM: BadOrderService has four methods, each beginning with the same
// switch (order.Status) block. The same status logic is copy-pasted in four places.
//
// Two pain points demonstrated:
//   1. Adding a new "ReturnRequested" status requires updating 4 separate
//      switch blocks — every location is marked ← ADD HERE.
//   2. Cancelling a Delivered order silently does nothing — the default branch
//      just breaks without logging or throwing, leaving no trace of the invalid
//      transition attempt.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: BadOrderService — switch blocks duplicated across 4 methods ===");
Log.Information("");

var svc   = new BadOrderService();
var order = new Order("ORD-001", "CUST-42", 149.99m);

Log.Information("--- Happy path: Pending → Confirmed → Shipped → Delivered ---");
svc.Confirm(order);
svc.Ship(order);
svc.Deliver(order);
Log.Information("  Final status: {Status}", order.Status);
Log.Information("");

Log.Information("--- Invalid transition: Cancel a Delivered order ---");
svc.Cancel(order);  // silently does nothing — no exception, no log
Log.Information("  Status after cancel attempt: {Status} (unchanged — silent failure)", order.Status);
Log.Information("");

Log.Information(">>> To add 'ReturnRequested' status:");
Log.Information("     1. Add 'case \"Delivered\":' to Confirm() switch  ← ADD HERE");
Log.Information("     2. Add 'case \"Delivered\":' to Ship() switch     ← ADD HERE");
Log.Information("     3. Add 'case \"Delivered\":' to Cancel() switch   ← ADD HERE");
Log.Information("     4. Add 'case \"ReturnRequested\":' to Deliver()   ← ADD HERE");
Log.Information(">>> Fix: State pattern — each state is a class that knows its own transitions.");

// ─── Domain ───────────────────────────────────────────────────────────────────

public class Order(string orderId, string customerId, decimal amount)
{
    public string  OrderId    { get; } = orderId;
    public string  CustomerId { get; } = customerId;
    public decimal Amount     { get; } = amount;
    public string  Status     { get; set; } = "Pending";
}

// PROBLEM: 4 methods × the same switch — 40+ lines of duplication.
// Every new status must be added in 4 separate places.
public class BadOrderService
{
    public void Confirm(Order order)
    {
        switch (order.Status)          // ← ADD ReturnRequested case HERE (1 of 4)
        {
            case "Pending":
                order.Status = "Confirmed";
                Log.Information("  [Confirm] Pending → Confirmed");
                break;
            case "Confirmed":
                Log.Warning("  [Confirm] Already confirmed.");
                break;
            case "Shipped":
            case "Delivered":
            case "Cancelled":
                Log.Warning("  [Confirm] Cannot confirm — status is {Status}", order.Status);
                break;
            default:
                break; // silently swallows unknown statuses
        }
    }

    public void Ship(Order order)
    {
        switch (order.Status)          // ← ADD ReturnRequested case HERE (2 of 4)
        {
            case "Confirmed":
                order.Status = "Shipped";
                Log.Information("  [Ship] Confirmed → Shipped");
                break;
            case "Pending":
                Log.Warning("  [Ship] Cannot ship — order not confirmed.");
                break;
            case "Shipped":
            case "Delivered":
            case "Cancelled":
                Log.Warning("  [Ship] Cannot ship — status is {Status}", order.Status);
                break;
            default:
                break;
        }
    }

    public void Deliver(Order order)
    {
        switch (order.Status)          // ← ADD ReturnRequested case HERE (3 of 4)
        {
            case "Shipped":
                order.Status = "Delivered";
                Log.Information("  [Deliver] Shipped → Delivered");
                break;
            case "Pending":
            case "Confirmed":
            case "Cancelled":
                Log.Warning("  [Deliver] Cannot deliver — status is {Status}", order.Status);
                break;
            default:
                break;
        }
    }

    public void Cancel(Order order)
    {
        switch (order.Status)          // ← ADD ReturnRequested case HERE (4 of 4)
        {
            case "Pending":
            case "Confirmed":
                order.Status = "Cancelled";
                Log.Information("  [Cancel] → Cancelled");
                break;
            case "Shipped":
            case "Delivered":
                // PROBLEM: silently does nothing — no exception, no log.
                // Caller has no idea the cancel was ignored.
                break;
            default:
                break;
        }
    }
}
