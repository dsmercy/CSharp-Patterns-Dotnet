namespace Mediator.App.Models;

// All events that colleagues can raise through the mediator.
// The mediator's NotifyAsync switch covers each one.
public enum CheckoutEvent
{
    PaymentSucceeded,
    PaymentFailed,
    PaymentRefunded,
    StockReserved,
    StockReleased,
    ShipmentBooked,
    ConfirmationSent,
    RefundConfirmationSent,
    PaymentFailureNotificationSent
}
