using Mediator.App.Models;
using Serilog;

namespace Mediator.App.Colleagues;

// PATTERN CONCEPT: Concrete Colleague — notifications.
//
// Sends different emails depending on which event the mediator asked it to handle.
// Has NO back-reference to PaymentComponent — the circular dependency from the
// Problem version cannot occur here.
public class NotificationComponent(ICheckoutMediator mediator) : CheckoutColleague(mediator)
{
    public async Task SendConfirmationAsync(Order order)
    {
        await Task.Delay(20);
        Log.Information("  [Notification] Order confirmation sent to customer {Id}",
            order.CustomerId);
        await _mediator.NotifyAsync(this, CheckoutEvent.ConfirmationSent, order);
    }

    public async Task SendPaymentFailureAsync(Order order)
    {
        await Task.Delay(20);
        Log.Warning("  [Notification] Payment failure email sent to customer {Id}",
            order.CustomerId);
        await _mediator.NotifyAsync(this, CheckoutEvent.PaymentFailureNotificationSent, order);
    }

    public async Task SendRefundConfirmationAsync(Order order)
    {
        await Task.Delay(20);
        Log.Information("  [Notification] Refund confirmation sent to customer {Id}",
            order.CustomerId);
        await _mediator.NotifyAsync(this, CheckoutEvent.RefundConfirmationSent, order);
    }
}
