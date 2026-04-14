using Strategy.App.Models;

namespace Strategy.App.Strategies;

// 10% of the order value, next-day delivery.
public class ExpressShippingStrategy : IShippingStrategy
{
    public string  StrategyName  => "Express";
    public int     EstimatedDays => 1;
    public decimal Calculate(Order order) => Math.Round(order.Amount * 0.10m, 2);
}
