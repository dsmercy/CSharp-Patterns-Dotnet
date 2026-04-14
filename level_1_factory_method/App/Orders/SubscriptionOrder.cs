using Serilog;

namespace FactoryMethod.App.Orders;

// Concrete Product: recurring billing — sets the next renewal date.
public class SubscriptionOrder : Order
{
    public DateTime RenewalDate { get; }

    public SubscriptionOrder(string customerId, string product, decimal amount)
        : base(customerId, product, amount)
    {
        RenewalDate = DateTime.UtcNow.AddMonths(1);
    }

    public override void Process()
    {
        Log.Information("  [SubscriptionOrder] Setting up recurring billing for customer {CustomerId}", CustomerId);
        Log.Information("  [SubscriptionOrder] Next renewal: {Date:yyyy-MM-dd}", RenewalDate);
        Log.Information("  [SubscriptionOrder] Activating subscription for '{Product}'", Product);
    }
}
