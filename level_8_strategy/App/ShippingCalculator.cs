using Strategy.App.Models;

namespace Strategy.App;

// PATTERN CONCEPT: the Context.
//
// Holds a reference to IShippingStrategy and delegates Calculate() to it.
// Contains ZERO knowledge of Standard, Express, Free, or SameDay shipping.
// Adding a new strategy requires only a new class — ShippingCalculator never changes.
//
// SetStrategy() allows the strategy to be swapped at runtime — e.g. when a user
// changes their delivery option on the checkout page without reloading.
public class ShippingCalculator(IShippingStrategy strategy)
{
    private IShippingStrategy _strategy = strategy;

    // PATTERN CONCEPT: runtime strategy swap — no need to create a new calculator.
    public void SetStrategy(IShippingStrategy strategy) => _strategy = strategy;

    public IShippingStrategy CurrentStrategy => _strategy;

    public ShippingQuote GetQuote(Order order) =>
        new(_strategy.StrategyName, _strategy.Calculate(order), _strategy.EstimatedDays);
}
