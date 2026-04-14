namespace State.App.States;

// PATTERN CONCEPT: Concrete State — Delivered (terminal state).
//
// No further transitions are allowed. All operations throw.
// In the Problem version, Cancel silently did nothing here.
// Here it throws explicitly — the caller always knows the attempt failed.
public class DeliveredState : IOrderState
{
    public string StateName => "Delivered";

    public void Confirm(Order order) =>
        throw new InvalidOperationException(
            "Cannot confirm order — it has already been delivered.");

    public void Ship(Order order) =>
        throw new InvalidOperationException(
            "Cannot ship order — it has already been delivered.");

    public void Deliver(Order order) =>
        throw new InvalidOperationException(
            "Cannot deliver order — it has already been delivered.");

    public void Cancel(Order order) =>
        // PATTERN CONCEPT: invalid transition throws — never silently ignored.
        throw new InvalidOperationException(
            "Cannot cancel order — it has already been delivered.");
}
