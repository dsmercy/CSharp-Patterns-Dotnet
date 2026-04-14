namespace State.App.States;

// PATTERN CONCEPT: Concrete State — Confirmed.
//
// Allowed transitions: Ship → Shipped, Cancel → Cancelled.
// Disallowed: Confirm (already confirmed), Deliver (must ship first).
public class ConfirmedState : IOrderState
{
    public string StateName => "Confirmed";

    public void Ship(Order order) =>
        order.TransitionTo(new ShippedState());

    public void Cancel(Order order) =>
        order.TransitionTo(new CancelledState());

    public void Confirm(Order order) =>
        throw new InvalidOperationException(
            "Cannot confirm order — it is already confirmed.");

    public void Deliver(Order order) =>
        throw new InvalidOperationException(
            "Cannot deliver order — it has not been shipped yet.");
}
