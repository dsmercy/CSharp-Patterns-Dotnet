using Observer.App.Models;
using Serilog;

namespace Observer.App.Observers;

// PATTERN CONCEPT: Concrete Observer — records business metrics.
//
// Completely independent of InventoryObserver and NotificationObserver.
// Can be subscribed or unsubscribed at runtime (e.g. disabled in maintenance mode).
public class AnalyticsObserver : IOrderObserver
{
    private int _placedCount;
    private int _cancelledCount;

    public async Task OnOrderEventAsync(OrderEvent orderEvent)
    {
        await Task.Delay(20); // simulate analytics write

        switch (orderEvent.EventType)
        {
            case "OrderPlaced":
                _placedCount++;
                Log.Information("  [AnalyticsObserver] Order placed — running total: {Count} placed",
                    _placedCount);
                break;

            case "OrderCancelled":
                _cancelledCount++;
                Log.Information("  [AnalyticsObserver] Order cancelled — running total: {Count} cancelled",
                    _cancelledCount);
                break;
        }
    }
}
