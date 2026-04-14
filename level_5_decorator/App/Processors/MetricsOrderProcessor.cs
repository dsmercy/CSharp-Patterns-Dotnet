using System.Diagnostics;
using Decorator.App.Models;
using Serilog;

namespace Decorator.App.Processors;

// PATTERN CONCEPT: Concrete Decorator C — execution time metrics.
// Measures and records how long the inner chain takes.
// Completely independent of logging or validation — compose only when needed.
public class MetricsOrderProcessor(IOrderProcessor inner) : OrderProcessorDecorator(inner)
{
    public override async Task<ProcessingResult> ProcessAsync(Order order)
    {
        var sw = Stopwatch.StartNew();

        var result = await _inner.ProcessAsync(order);

        sw.Stop();
        Log.Information("  [Metrics] order {Id} completed in {Ms}ms — success: {Success}",
            order.OrderId, sw.ElapsedMilliseconds, result.Success);

        return result;
    }
}
