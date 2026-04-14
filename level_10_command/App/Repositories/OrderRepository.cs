using Command.App.Models;
using Serilog;

namespace Command.App.Repositories;

// Simulates a persistence store. Commands call into this — the Invoker
// never touches it directly.
public class OrderRepository
{
    private readonly Dictionary<string, Order> _store = [];

    public Task SaveAsync(Order order)
    {
        _store[order.OrderId] = order;
        Log.Information("  [Repository] Saved {Order}", order);
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
