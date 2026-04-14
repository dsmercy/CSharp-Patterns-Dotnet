using Decorator.App.Models;

namespace Decorator.App.Processors;

// PATTERN CONCEPT: the abstract Base Decorator.
//
// Holds a reference to any IOrderProcessor (_inner) and delegates to it by default.
// Concrete decorators inherit from this and override ProcessAsync to add behaviour
// before and/or after the _inner.ProcessAsync(order) call.
//
// This eliminates the delegation boilerplate that was copied into every subclass in
// the Problem project — each concrete decorator writes only the NEW behaviour.
public abstract class OrderProcessorDecorator(IOrderProcessor inner) : IOrderProcessor
{
    protected readonly IOrderProcessor _inner = inner;

    // Default: pure delegation. Concrete decorators override to add behaviour.
    public virtual Task<ProcessingResult> ProcessAsync(Order order)
        => _inner.ProcessAsync(order);
}
