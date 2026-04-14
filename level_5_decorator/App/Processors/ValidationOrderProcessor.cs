using Decorator.App.Models;
using Serilog;

namespace Decorator.App.Processors;

// PATTERN CONCEPT: Concrete Decorator B — input validation.
// Rejects invalid orders before they reach the inner processor.
// Completely independent of logging or metrics — compose only when needed.
public class ValidationOrderProcessor(IOrderProcessor inner) : OrderProcessorDecorator(inner)
{
    public override async Task<ProcessingResult> ProcessAsync(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.CustomerId))
        {
            Log.Warning("  [Validation] REJECTED order {Id} — missing customer ID", order.OrderId);
            return new ProcessingResult(order.OrderId, false, "Validation failed: missing customer ID");
        }

        if (order.Amount <= 0)
        {
            Log.Warning("  [Validation] REJECTED order {Id} — amount must be positive", order.OrderId);
            return new ProcessingResult(order.OrderId, false, "Validation failed: amount must be positive");
        }

        Log.Information("  [Validation] PASSED — order {Id}", order.OrderId);
        return await _inner.ProcessAsync(order);
    }
}
