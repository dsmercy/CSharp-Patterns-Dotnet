namespace State.App.States;

// PATTERN CONCEPT: Concrete State — Cancelled (terminal state).
//
// Once cancelled, no transitions are allowed. All operations throw.
public class CancelledState : IOrderState
{
    public string StateName => "Cancelled";

    public void Confirm(Order order) =>
        throw new InvalidOperationException(
            "Cannot confirm order — it has been cancelled.");

    public void Ship(Order order) =>
        throw new InvalidOperationException(
            "Cannot ship order — it has been cancelled.");

    public void Deliver(Order order) =>
        throw new InvalidOperationException(
            "Cannot deliver order — it has been cancelled.");

    public void Cancel(Order order) =>
        throw new InvalidOperationException(
            "Cannot cancel order — it is already cancelled.");
}
