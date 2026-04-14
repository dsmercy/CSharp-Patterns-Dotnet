using FactoryMethod.App.Orders;

namespace FactoryMethod.App.Factories;

// Concrete Creator: knows only how to create a DigitalOrder.
// Adding a new order type never touches this class.
public class DigitalOrderFactory : OrderFactory
{
    public override Order CreateOrder(string customerId, string product, decimal amount)
        => new DigitalOrder(customerId, product, amount);
}
