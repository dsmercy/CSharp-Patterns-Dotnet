using FactoryMethod.App.Orders;

namespace FactoryMethod.App.Factories;

// Concrete Creator: knows only how to create a PhysicalOrder.
public class PhysicalOrderFactory : OrderFactory
{
    public override Order CreateOrder(string customerId, string product, decimal amount)
        => new PhysicalOrder(customerId, product, amount);
}
