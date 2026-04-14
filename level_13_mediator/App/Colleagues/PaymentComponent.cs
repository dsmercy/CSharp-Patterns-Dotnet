using Mediator.App.Models;
using Serilog;

namespace Mediator.App.Colleagues;

// PATTERN CONCEPT: Concrete Colleague — payment.
//
// Knows only ICheckoutMediator. Has no reference to Inventory, Shipping, or
// Notification. It charges the card, then signals the mediator — the mediator
// decides what happens next.
public class PaymentComponent(ICheckoutMediator mediator) : CheckoutColleague(mediator)
{
    public async Task ProcessPaymentAsync(Order order)
    {
        await Task.Delay(50); // simulate payment gateway API
        Log.Information("  [Payment]      Charged £{Amount:F2} for order {Id}",
            order.Amount, order.OrderId);

        // PATTERN CONCEPT: signal mediator — do NOT call InventoryComponent directly.
        await _mediator.NotifyAsync(this, CheckoutEvent.PaymentSucceeded, order);
    }

    public async Task ProcessFailedPaymentAsync(Order order)
    {
        await Task.Delay(50);
        Log.Warning("  [Payment]      FAILED — card declined for order {Id}", order.OrderId);
        await _mediator.NotifyAsync(this, CheckoutEvent.PaymentFailed, order);
    }

    public async Task RefundAsync(Order order)
    {
        await Task.Delay(40);
        Log.Information("  [Payment]      Refunded £{Amount:F2} for order {Id}",
            order.Amount, order.OrderId);
        await _mediator.NotifyAsync(this, CheckoutEvent.PaymentRefunded, order);
    }
}
