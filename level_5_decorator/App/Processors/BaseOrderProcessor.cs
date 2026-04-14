using Decorator.App.Models;
using Serilog;

namespace Decorator.App.Processors;

// PATTERN CONCEPT: the Concrete Component.
// Contains the real business logic — persisting the order.
// Has no logging, no validation, no metrics. Each concern is added separately via decorators.
public class BaseOrderProcessor : IOrderProcessor
{
    public async Task<ProcessingResult> ProcessAsync(Order order)
    {
        await Task.Delay(60); // simulate async DB write
        Log.Information("    [Base] Order {Id} persisted to database (£{Amount:F2})",
            order.OrderId, order.Amount);
        return new ProcessingResult(order.OrderId, true, "Order persisted");
    }
}
