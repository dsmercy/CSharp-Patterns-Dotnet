namespace State.App.States;

// PATTERN CONCEPT: Concrete State — Shipped.
//
// Allowed transitions: Deliver → Delivered.
// Disallowed: Confirm, Ship, Cancel — all throw with specific messages.
// Note: cancellation is blocked once shipped — the courier has the package.
//
// States can hold their own data. ShippedState could store a TrackingNumber
// property — the Order class would not need to change at all.
public class ShippedState : IOrderState
{
    public string StateName => "Shipped";

    public void Deliver(Order order) =>
        order.TransitionTo(new DeliveredState());

    public void Confirm(Order order) =>
        throw new InvalidOperationException(
            "Cannot confirm order — it has already been shipped.");

    public void Ship(Order order) =>
        throw new InvalidOperationException(
            "Cannot ship order — it has already been shipped.");

    public void Cancel(Order order) =>
        throw new InvalidOperationException(
            "Cannot cancel order — it has already been shipped.");
}
