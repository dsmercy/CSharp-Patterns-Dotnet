using Mediator.App.Colleagues;
using Mediator.App.Models;
using Serilog;

namespace Mediator.App;

// PATTERN CONCEPT: Concrete Mediator — the ONLY class that knows all colleagues
//                  and owns ALL coordination/sequencing logic.
//
// Trade-off: the mediator is intentionally a "hub" — it knows everything about
// the checkout workflow. Keep it focused on ONE workflow. Do not merge checkout,
// returns, and loyalty into a single mediator or it becomes a god object.
//
// Adding LoyaltyComponent: add one new case to NotifyAsync. No existing
// colleague changes.
//
// Real-world equivalent: MediatR's IMediator — IRequest = the message,
// IRequestHandler = the colleague, IMediator.Send() = this NotifyAsync.
public class CheckoutMediator : ICheckoutMediator
{
    // PATTERN CONCEPT: mediator holds references to ALL colleagues.
    // Colleagues do NOT hold references to each other.
    private readonly PaymentComponent      _payment;
    private readonly InventoryComponent    _inventory;
    private readonly ShippingComponent     _shipping;
    private readonly NotificationComponent _notification;

    public CheckoutMediator()
    {
        // PATTERN CONCEPT: mediator creates colleagues, injecting itself so they
        // can signal back. The circular reference is intentional and safe here —
        // the mediator is fully constructed before any colleague calls it.
        _payment      = new PaymentComponent(this);
        _inventory    = new InventoryComponent(this);
        _shipping     = new ShippingComponent(this);
        _notification = new NotificationComponent(this);
    }

    // PATTERN CONCEPT: all coordination logic lives here — colleagues never
    // coordinate directly. Sequencing is explicit and visible in one place.
    public async Task NotifyAsync(
        CheckoutColleague sender, CheckoutEvent checkoutEvent, Order order)
    {
        switch (checkoutEvent)
        {
            case CheckoutEvent.PaymentSucceeded:
                // Payment done → reserve stock
                await _inventory.ReserveStockAsync(order);
                break;

            case CheckoutEvent.StockReserved:
                // Stock reserved → book shipment
                await _shipping.BookShipmentAsync(order);
                break;

            case CheckoutEvent.ShipmentBooked:
                // Shipment booked → send confirmation
                await _notification.SendConfirmationAsync(order);
                break;

            case CheckoutEvent.PaymentFailed:
                // Payment failed → notify customer only (no stock or shipping)
                await _notification.SendPaymentFailureAsync(order);
                break;

            case CheckoutEvent.PaymentRefunded:
                // Refund issued → release stock AND notify customer
                await _inventory.ReleaseStockAsync(order);
                await _notification.SendRefundConfirmationAsync(order);
                break;

            default:
                // Terminal events (ConfirmationSent, StockReleased, etc.) — chain ends.
                Log.Information("  [Mediator]     '{Event}' — workflow complete", checkoutEvent);
                break;
        }
    }

    // ── Entry points for external callers ────────────────────────────────────
    // Callers only need these three methods — they never touch colleagues directly.

    public Task CheckoutAsync(Order order) =>
        _payment.ProcessPaymentAsync(order);

    public Task CheckoutFailedAsync(Order order) =>
        _payment.ProcessFailedPaymentAsync(order);

    public Task RefundAsync(Order order) =>
        _payment.RefundAsync(order);
}
