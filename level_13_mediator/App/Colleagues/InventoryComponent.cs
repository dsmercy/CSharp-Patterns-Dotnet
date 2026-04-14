using Mediator.App.Models;
using Serilog;

namespace Mediator.App.Colleagues;

// PATTERN CONCEPT: Concrete Colleague — inventory.
//
// Has no reference to ShippingComponent or NotificationComponent.
// Reserves or releases stock, then signals the mediator.
public class InventoryComponent(ICheckoutMediator mediator) : CheckoutColleague(mediator)
{
    public async Task ReserveStockAsync(Order order)
    {
        await Task.Delay(30);
        Log.Information("  [Inventory]    Stock reserved for '{Product}'", order.Product);
        await _mediator.NotifyAsync(this, CheckoutEvent.StockReserved, order);
    }

    public async Task ReleaseStockAsync(Order order)
    {
        await Task.Delay(30);
        Log.Information("  [Inventory]    Stock released for '{Product}'", order.Product);
        await _mediator.NotifyAsync(this, CheckoutEvent.StockReleased, order);
    }
}
