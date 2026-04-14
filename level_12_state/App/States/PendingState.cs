namespace State.App.States;

// PATTERN CONCEPT: Concrete State — Pending.
//
// Allowed transitions: Confirm → Confirmed, Cancel → Cancelled.
// Disallowed: Ship, Deliver — both throw immediately with a clear message.
public class PendingState : IOrderState
{
    public string StateName => "Pending";

    public void Confirm(Order order) =>
        order.TransitionTo(new ConfirmedState());

    public void Cancel(Order order) =>
        order.TransitionTo(new CancelledState());

    public void Ship(Order order) =>
        throw new InvalidOperationException(
            "Cannot ship order — it has not been confirmed yet.");

    public void Deliver(Order order) =>
        throw new InvalidOperationException(
            "Cannot deliver order — it has not been confirmed or shipped.");
}
