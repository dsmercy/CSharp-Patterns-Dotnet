using Observer.App.Models;
using Serilog;

namespace Observer.App;

// PATTERN CONCEPT: Subject (Observable).
//
// Maintains a list of IOrderObserver subscribers and broadcasts events to all
// of them. It knows nothing about what each observer does — it just iterates
// and calls OnOrderEventAsync.
//
// Key design decisions:
//   - Subscribe / Unsubscribe are runtime operations — observers can be added
//     or removed while the application is running.
//   - NotifyAsync catches per-observer exceptions so one failing observer
//     (e.g. a broken email service) cannot abort the rest.
//   - The Subject fires two event types: "OrderPlaced" and "OrderCancelled".
public class OrderService
{
    private readonly List<IOrderObserver> _observers = [];

    // ── Subscription management ───────────────────────────────────────────────

    public void Subscribe(IOrderObserver observer)
    {
        _observers.Add(observer);
        Log.Information("  [OrderService] Subscribed: {Type}", observer.GetType().Name);
    }

    public void Unsubscribe(IOrderObserver observer)
    {
        _observers.Remove(observer);
        Log.Information("  [OrderService] Unsubscribed: {Type}", observer.GetType().Name);
    }

    // ── Business operations ───────────────────────────────────────────────────

    public async Task PlaceOrderAsync(Order order)
    {
        Log.Information("  [OrderService] Placing order {Id} — £{Amount:F2}", order.OrderId, order.Amount);
        // ... save to DB, validate stock, etc.
        await NotifyAsync(new OrderEvent("OrderPlaced", order, DateTime.UtcNow));
    }

    public async Task CancelOrderAsync(Order order)
    {
        Log.Information("  [OrderService] Cancelling order {Id}", order.OrderId);
        // ... update status in DB, etc.
        await NotifyAsync(new OrderEvent("OrderCancelled", order, DateTime.UtcNow));
    }

    // ── Notification broadcast ────────────────────────────────────────────────

    // PATTERN CONCEPT: per-observer exception handling.
    // If NotificationObserver throws (e.g. SMTP down), InventoryObserver and
    // AnalyticsObserver still run. One broken observer cannot block the others.
    private async Task NotifyAsync(OrderEvent orderEvent)
    {
        Log.Information("  [OrderService] Broadcasting '{Event}' to {Count} observer(s)",
            orderEvent.EventType, _observers.Count);

        foreach (var observer in _observers.ToList()) // ToList — safe against mid-loop Unsubscribe
        {
            try
            {
                await observer.OnOrderEventAsync(orderEvent);
            }
            catch (Exception ex)
            {
                Log.Error("  [OrderService] Observer {Type} threw — continuing. Error: {Msg}",
                    observer.GetType().Name, ex.Message);
            }
        }
    }
}
