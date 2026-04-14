using Observer.App.Models;

namespace Observer.App;

// PATTERN CONCEPT: Observer interface — the Subscriber contract.
//
// Any class that wants to react to order events implements this interface.
// The Subject (OrderService) depends only on this abstraction — it has no
// knowledge of InventoryObserver, NotificationObserver, or AnalyticsObserver.
public interface IOrderObserver
{
    Task OnOrderEventAsync(OrderEvent orderEvent);
}

// The event object carried to every subscriber.
// Using a record keeps it immutable — observers cannot mutate shared state.
public record OrderEvent(string EventType, Order Order, DateTime OccurredAt);
