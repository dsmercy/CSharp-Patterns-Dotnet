using Strategy.App.Models;

namespace Strategy.App;

public record ShippingQuote(string StrategyName, decimal Cost, int EstimatedDays)
{
    public override string ToString() =>
        $"{StrategyName,-10} → £{Cost:F2}, {EstimatedDays} day(s)";
}

// PATTERN CONCEPT: the Strategy interface.
// Every shipping algorithm implements this contract.
// ShippingCalculator (the Context) depends only on this interface,
// so any strategy can be injected or swapped without touching the calculator.
public interface IShippingStrategy
{
    string  StrategyName  { get; }
    int     EstimatedDays { get; }
    decimal Calculate(Order order);
}
