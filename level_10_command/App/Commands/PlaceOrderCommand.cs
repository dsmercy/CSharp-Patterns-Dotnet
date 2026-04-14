using Command.App.Models;
using Command.App.Repositories;
using Serilog;

namespace Command.App.Commands;

// PATTERN CONCEPT: Concrete Command.
//
// Stores the Receiver (OrderRepository) and the Order needed to execute the
// action. Undo deletes the order from the repository — the inverse operation.
public class PlaceOrderCommand(OrderRepository repository, Order order) : IOrderCommand
{
    public string Description => $"Place order {order.OrderId} (£{order.Amount:F2})";

    public async Task ExecuteAsync()
    {
        await repository.SaveAsync(order);
        Log.Information("  [PlaceOrderCommand] Executed — {Desc}", Description);
    }

    public async Task UndoAsync()
    {
        await repository.DeleteAsync(order.OrderId);
        Log.Information("  [PlaceOrderCommand] Undone  — removed order {Id}", order.OrderId);
    }
}
