namespace State.App;

// PATTERN CONCEPT: State interface.
//
// Every state must handle all four operations. States that disallow an operation
// throw InvalidOperationException with a clear message — no silent failures.
public interface IOrderState
{
    void   Confirm(Order order);
    void   Ship(Order order);
    void   Deliver(Order order);
    void   Cancel(Order order);
    string StateName { get; }
}
