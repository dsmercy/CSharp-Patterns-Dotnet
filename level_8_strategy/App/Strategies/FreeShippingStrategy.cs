using Strategy.App.Models;

namespace Strategy.App.Strategies;

// £0 shipping — only valid when the order exceeds the minimum threshold.
public class FreeShippingStrategy : IShippingStrategy
{
    private const decimal MinimumOrderAmount = 50m;

    public string  StrategyName  => "Free";
    public int     EstimatedDays => 5;

    public decimal Calculate(Order order)
    {
        if (order.Amount < MinimumOrderAmount)
            throw new InvalidOperationException(
                $"Free shipping requires a minimum order of £{MinimumOrderAmount:F2}. " +
                $"Order total is £{order.Amount:F2}.");
        return 0m;
    }
}
