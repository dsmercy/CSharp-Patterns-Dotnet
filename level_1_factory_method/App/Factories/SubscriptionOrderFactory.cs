using FactoryMethod.App.Orders;

namespace FactoryMethod.App.Factories;

// Concrete Creator: knows only how to create a SubscriptionOrder.
public class SubscriptionOrderFactory : OrderFactory
{
    public override Order CreateOrder(string customerId, string product, decimal amount)
        => new SubscriptionOrder(customerId, product, amount);
}
