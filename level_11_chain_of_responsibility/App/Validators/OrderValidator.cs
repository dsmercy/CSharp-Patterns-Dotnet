using Chain.App.Models;

namespace Chain.App.Validators;

// PATTERN CONCEPT: Abstract Handler.
//
// Each concrete validator either stops the chain (returns Fail) or passes
// the request to the next handler via PassToNextAsync.
//
// SetNext returns the next handler so the chain can be assembled fluently:
//   stock.SetNext(payment).SetNext(fraud).SetNext(address)
//
// Real-world parallel: ASP.NET Core Middleware — each middleware either
// short-circuits (returns a response) or calls next() to continue the pipeline.
public abstract class OrderValidator
{
    private OrderValidator? _next;

    public OrderValidator SetNext(OrderValidator next)
    {
        _next = next;
        return next; // enables fluent chaining
    }

    public abstract Task<ValidationResult> ValidateAsync(Order order);

    // PATTERN CONCEPT: pass to next handler, or succeed if this is the last one.
    protected Task<ValidationResult> PassToNextAsync(Order order) =>
        _next is null
            ? Task.FromResult(ValidationResult.Success())
            : _next.ValidateAsync(order);
}
