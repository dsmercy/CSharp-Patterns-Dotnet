using Command.App.Models;
using Command.App.Repositories;
using Serilog;

namespace Command.App.Commands;

// PATTERN CONCEPT: Concrete Command — status transition with undo.
//
// Captures the previous status so UndoAsync can revert to it, regardless of
// what the status was before the cancellation was issued.
public class CancelOrderCommand(OrderRepository repository, Order order) : IOrderCommand
{
    private string _previousStatus = order.Status;

    public string Description => $"Cancel order {order.OrderId}";

    public async Task ExecuteAsync()
    {
        _previousStatus = order.Status;
        await repository.UpdateStatusAsync(order.OrderId, "Cancelled");
        order.Status = "Cancelled";
        Log.Information("  [CancelOrderCommand] Executed — order {Id} cancelled", order.OrderId);
    }

    public async Task UndoAsync()
    {
        await repository.UpdateStatusAsync(order.OrderId, _previousStatus);
        order.Status = _previousStatus;
        Log.Information("  [CancelOrderCommand] Undone  — order {Id} reverted to '{Status}'",
            order.OrderId, _previousStatus);
    }
}
