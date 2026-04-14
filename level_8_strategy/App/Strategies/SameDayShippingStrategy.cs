using Strategy.App.Models;

namespace Strategy.App.Strategies;

// PATTERN CONCEPT: adding a new strategy = one new class.
// ShippingCalculator (the Context) is untouched. No switch to modify.
// Only available for orders placed before 12:00.
public class SameDayShippingStrategy : IShippingStrategy
{
    private const decimal FlatRate   = 14.99m;
    private const int     CutoffHour = 12;

    public string  StrategyName  => "SameDay";
    public int     EstimatedDays => 0;

    public decimal Calculate(Order order)
    {
        if (DateTime.UtcNow.Hour >= CutoffHour)
            throw new InvalidOperationException(
                $"Same-day shipping must be ordered before {CutoffHour:D2}:00 UTC. " +
                $"Current UTC hour: {DateTime.UtcNow.Hour:D2}.");
        return FlatRate;
    }
}
