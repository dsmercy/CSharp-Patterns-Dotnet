using Command.App.Models;
using Command.App.Repositories;
using Serilog;

namespace Command.App.Commands;

// PATTERN CONCEPT: Concrete Command with stored undo state.
//
// _previousAmount captures the order total before the discount is applied.
// UndoAsync restores it — without this captured snapshot, undo is impossible.
// This is the key insight: the Command object owns the state needed for reversal.
public class ApplyDiscountCommand(
    OrderRepository repository,
    Order           order,
    decimal         discountPercent) : IOrderCommand
{
    private decimal _previousAmount;

    public string Description =>
        $"Apply {discountPercent}% discount to order {order.OrderId}";

    public async Task ExecuteAsync()
    {
        _previousAmount = order.Amount;                        // snapshot for undo
        order.Amount   *= 1 - discountPercent / 100m;
        await repository.SaveAsync(order);
        Log.Information("  [ApplyDiscountCommand] Executed — £{Prev:F2} → £{New:F2}",
            _previousAmount, order.Amount);
    }

    public async Task UndoAsync()
    {
        order.Amount = _previousAmount;                        // restore snapshot
        await repository.SaveAsync(order);
        Log.Information("  [ApplyDiscountCommand] Undone  — £{Amount:F2} restored",
            _previousAmount);
    }
}
