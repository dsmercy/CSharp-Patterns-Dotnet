// PROBLEM: BadOrderAgent calls methods directly — no undo capability, no history.
//
// Two pain points demonstrated:
//   1. Undo request throws NotImplementedException — there is no record of what
//      changed, so rolling back requires knowing exactly what each method did
//      and manually reversing it by hand.
//   2. History log returns an empty list — there is nothing to audit or replay.
//      The agent must re-fetch current order state from scratch to assess what
//      happened.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: BadOrderAgent — direct method calls, no undo or history ===");
Log.Information("");

var repository = new OrderRepository();
var agent      = new BadOrderAgent(repository);
var order      = new Order("ORD-001", "CUST-42", 200.00m);

Log.Information("--- Agent places order and applies discount ---");
await agent.PlaceOrderAsync(order);
await agent.ApplyDiscountAsync(order, 10m);
Log.Information("  Order amount after 10% discount: £{Amount:F2}", order.Amount);
Log.Information("");

Log.Information("--- Agent requests undo (wrong discount code) ---");
try
{
    agent.Undo();
}
catch (NotImplementedException ex)
{
    Log.Error("  UNDO FAILED: {Message}", ex.Message);
}
Log.Information("");

Log.Information("--- Agent requests history log ---");
var history = agent.GetHistory();
if (!history.Any())
    Log.Warning("  History is empty — no record of what was done.");
Log.Information("");

Log.Information(">>> To support undo, BadOrderAgent must manually reverse each operation.");
Log.Information(">>> To support history, caller must build its own audit trail.");
Log.Information(">>> Fix: Command pattern — wrap each action as an IOrderCommand object.");

// ─── Domain ───────────────────────────────────────────────────────────────────

public class Order
{
    public string  OrderId    { get; }
    public string  CustomerId { get; }
    public decimal Amount     { get; set; }
    public string  Status     { get; set; } = "New";

    public Order(string orderId, string customerId, decimal amount)
    {
        OrderId    = orderId;
        CustomerId = customerId;
        Amount     = amount;
    }
}

public class OrderRepository
{
    private readonly Dictionary<string, Order> _store = [];

    public Task SaveAsync(Order order)
    {
        _store[order.OrderId] = order;
        Log.Information("  [Repository] Saved order {Id} — £{Amount:F2}", order.OrderId, order.Amount);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string orderId)
    {
        _store.Remove(orderId);
        Log.Information("  [Repository] Deleted order {Id}", orderId);
        return Task.CompletedTask;
    }

    public Task UpdateStatusAsync(string orderId, string status)
    {
        if (_store.TryGetValue(orderId, out var o)) o.Status = status;
        Log.Information("  [Repository] Status → {Status} for order {Id}", status, orderId);
        return Task.CompletedTask;
    }
}

// PROBLEM: direct calls — no undo stack, no history, no command objects.
public class BadOrderAgent(OrderRepository repository)
{
    // No _history field — nowhere to record what was done.

    public async Task PlaceOrderAsync(Order order)
    {
        await repository.SaveAsync(order);
        Log.Information("  [Agent] Order {Id} placed.", order.OrderId);
    }

    public async Task ApplyDiscountAsync(Order order, decimal percent)
    {
        order.Amount *= 1 - percent / 100m;
        await repository.SaveAsync(order);
        Log.Information("  [Agent] {Pct}% discount applied.", percent);
        // PROBLEM: _previousAmount not stored — undo is impossible without it
    }

    public void Undo()
        => throw new NotImplementedException(
               "No undo support — previous state was never captured.");

    public IEnumerable<string> GetHistory()
        => []; // PROBLEM: nothing was ever recorded
}
