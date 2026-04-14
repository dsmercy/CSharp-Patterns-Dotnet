using Decorator.App.Models;
using Serilog;

namespace Decorator.App.Processors;

// PATTERN CONCEPT: Concrete Decorator A — structured logging.
// Adds log lines before and after the inner call. Nothing else.
// Can wrap any IOrderProcessor — Base, Validation, Metrics, or another chain.
public class LoggingOrderProcessor(IOrderProcessor inner) : OrderProcessorDecorator(inner)
{
    public override async Task<ProcessingResult> ProcessAsync(Order order)
    {
        Log.Information("  [Logging] START — order {Id} (£{Amount:F2}) for customer {CustomerId}",
            order.OrderId, order.Amount, order.CustomerId);

        var result = await _inner.ProcessAsync(order);

        Log.Information("  [Logging] END   — order {Id}, success: {Success}, message: {Msg}",
            result.OrderId, result.Success, result.Message);

        return result;
    }
}
