using Observer.App.Models;
using Serilog;

namespace Observer.App.Observers;

// PATTERN CONCEPT: Concrete Observer.
//
// Reacts to order events by adjusting stock levels.
// It has no reference to OrderService — decoupled by the IOrderObserver contract.
public class InventoryObserver : IOrderObserver
{
    public async Task OnOrderEventAsync(OrderEvent orderEvent)
    {
        await Task.Delay(30); // simulate DB write

        switch (orderEvent.EventType)
        {
            case "OrderPlaced":
                Log.Information("  [InventoryObserver] Reserved stock for {Count} item(s) — order {Id}",
                    orderEvent.Order.Items.Length, orderEvent.Order.OrderId);
                break;

            case "OrderCancelled":
                Log.Information("  [InventoryObserver] Released stock for {Count} item(s) — order {Id}",
                    orderEvent.Order.Items.Length, orderEvent.Order.OrderId);
                break;
        }
    }
}
