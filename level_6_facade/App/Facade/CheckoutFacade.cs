using Facade.App.Models;
using Facade.App.Subsystems;
using Serilog;

namespace Facade.App.Facade;

// PATTERN CONCEPT: the Facade.
//
// Provides a single PlaceOrderAsync(cart) entry point that hides the 5-step
// subsystem orchestration from every call site.
//
// Key properties of a well-designed Facade:
//   - THIN: it coordinates and delegates — it contains no business logic itself.
//   - TRANSPARENT: callers can still reach subsystems directly if they need to;
//     the Facade makes the common case easy, not the only case possible.
//   - ONE SEQUENCE: when the checkout flow gains a 6th step (e.g. FraudService),
//     it is added here once. Every call site (web, mobile, admin) picks it up
//     automatically.
public class CheckoutFacade(
    InventoryService    inventory,
    PaymentService      payment,
    ShippingService     shipping,
    NotificationService notification,
    LoyaltyService      loyalty)
{
    public async Task<CheckoutResult> PlaceOrderAsync(Cart cart)
    {
        Log.Information("  [CheckoutFacade] Placing order for customer {Id} — £{Total:F2}",
            cart.CustomerId, cart.Total);

        // Step 1 — reserve stock
        await inventory.ReserveAsync(cart.Items);

        // Step 2 — charge payment
        var paymentConfirmation = await payment.ChargeAsync(cart.CustomerId, cart.Total);

        // Step 3 — book shipping
        var shipmentConfirmation = await shipping.BookAsync(cart.ShippingAddress, cart.Items);

        // Step 4 — send confirmation email/SMS
        await notification.SendConfirmationAsync(
            cart.CustomerId, paymentConfirmation, shipmentConfirmation);

        // Step 5 — award loyalty points
        var points = await loyalty.AwardPointsAsync(cart.CustomerId, cart.Total);

        // Step 6 would be added HERE — one edit, picked up by all call sites.

        Log.Information("  [CheckoutFacade] Order complete — txn: {Txn}, tracking: {Track}",
            paymentConfirmation.TransactionId, shipmentConfirmation.TrackingNumber);

        return new CheckoutResult(
            Success:              true,
            TransactionId:        paymentConfirmation.TransactionId,
            TrackingNumber:       shipmentConfirmation.TrackingNumber,
            LoyaltyPointsAwarded: points);
    }
}
