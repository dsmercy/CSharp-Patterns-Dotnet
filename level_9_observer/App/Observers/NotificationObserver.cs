using Observer.App.Models;
using Serilog;

namespace Observer.App.Observers;

// PATTERN CONCEPT: Concrete Observer — sends email/SMS notifications.
//
// This observer simulates an intermittent SMTP failure on cancellations.
// Because OrderService wraps each observer call in try/catch, this failure
// is isolated — InventoryObserver and AnalyticsObserver still run.
public class NotificationObserver : IOrderObserver
{
    private bool _simulateFailure;

    public NotificationObserver(bool simulateFailure = false)
        => _simulateFailure = simulateFailure;

    public async Task OnOrderEventAsync(OrderEvent orderEvent)
    {
        await Task.Delay(50); // simulate SMTP round-trip

        if (_simulateFailure && orderEvent.EventType == "OrderCancelled")
            throw new InvalidOperationException("SMTP connection refused — email not sent.");

        switch (orderEvent.EventType)
        {
            case "OrderPlaced":
                Log.Information("  [NotificationObserver] Confirmation email sent to customer {Id}",
                    orderEvent.Order.CustomerId);
                break;

            case "OrderCancelled":
                Log.Information("  [NotificationObserver] Cancellation email sent to customer {Id}",
                    orderEvent.Order.CustomerId);
                break;
        }
    }
}
