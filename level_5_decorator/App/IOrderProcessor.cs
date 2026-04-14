using Decorator.App.Models;

namespace Decorator.App;

public record ProcessingResult(string OrderId, bool Success, string Message = "");

// PATTERN CONCEPT: the Component interface.
// Both the real processor (BaseOrderProcessor) and every decorator implement this.
// The client — and every decorator — only ever depends on IOrderProcessor, so the
// chain can be arbitrarily deep without the client knowing.
public interface IOrderProcessor
{
    Task<ProcessingResult> ProcessAsync(Order order);
}
