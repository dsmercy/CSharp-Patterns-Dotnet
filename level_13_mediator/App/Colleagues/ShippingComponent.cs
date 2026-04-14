using Mediator.App.Models;
using Serilog;

namespace Mediator.App.Colleagues;

// PATTERN CONCEPT: Concrete Colleague — shipping.
//
// Has no reference to NotificationComponent. Books the shipment, signals mediator.
public class ShippingComponent(ICheckoutMediator mediator) : CheckoutColleague(mediator)
{
    public async Task BookShipmentAsync(Order order)
    {
        await Task.Delay(40);
        Log.Information("  [Shipping]     Shipment booked for order {Id}", order.OrderId);
        await _mediator.NotifyAsync(this, CheckoutEvent.ShipmentBooked, order);
    }
}
