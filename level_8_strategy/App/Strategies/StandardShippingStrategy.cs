using Strategy.App.Models;

namespace Strategy.App.Strategies;

// Flat rate £3.99, regardless of order value.
public class StandardShippingStrategy : IShippingStrategy
{
    public string  StrategyName  => "Standard";
    public int     EstimatedDays => 5;
    public decimal Calculate(Order order) => 3.99m;
}
