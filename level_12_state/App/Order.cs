using Serilog;
using State.App.States;

namespace State.App;

// PATTERN CONCEPT: Context.
//
// The Order class has zero switch blocks and zero status conditionals.
// All state-dependent behaviour is delegated to the current IOrderState.
// When a transition occurs, the state object itself calls TransitionTo()
// with the next state — the Order never decides which state to move to.
public class Order
{
    public string  OrderId    { get; init; }
    public string  CustomerId { get; init; }
    public decimal Amount     { get; init; }

    // PATTERN CONCEPT: the order's behaviour is defined entirely by this field.
    public IOrderState CurrentState { get; private set; } = new PendingState();

    public Order(string orderId, string customerId, decimal amount)
    {
        OrderId    = orderId;
        CustomerId = customerId;
        Amount     = amount;
    }

    public void TransitionTo(IOrderState newState)
    {
        Log.Information("  [{Id}] {From} → {To}",
            OrderId, CurrentState.StateName, newState.StateName);
        CurrentState = newState;
    }

    // Delegate every operation to the current state — no conditionals here.
    public void Confirm() => CurrentState.Confirm(this);
    public void Ship()    => CurrentState.Ship(this);
    public void Deliver() => CurrentState.Deliver(this);
    public void Cancel()  => CurrentState.Cancel(this);

    public override string ToString() =>
        $"Order({OrderId}, £{Amount:F2}, {CurrentState.StateName})";
}
