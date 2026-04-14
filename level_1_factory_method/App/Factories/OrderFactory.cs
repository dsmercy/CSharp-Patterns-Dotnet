using FactoryMethod.App.Orders;
using Serilog;

namespace FactoryMethod.App.Factories;

// PATTERN CONCEPT: the abstract Creator.
//
// It declares the factory method (CreateOrder) that subclasses must override.
// It also provides a template method (ProcessOrder) that orchestrates the work —
// the same steps for every order type, with the variation isolated to CreateOrder.
//
// Callers depend only on OrderFactory, never on a concrete factory or order class.
public abstract class OrderFactory
{
    // Factory method — subclasses decide which concrete Order to instantiate.
    public abstract Order CreateOrder(string customerId, string product, decimal amount);

    // Template method — identical for every order type.
    // The variation (which Order is created) is entirely inside CreateOrder.
    public void ProcessOrder(string customerId, string product, decimal amount)
    {
        Log.Information("--- Processing order for customer {CustomerId} ---", customerId);

        var order = CreateOrder(customerId, product, amount);

        Log.Information("Created {OrderType} [{OrderId}] — £{Amount:F2} for '{Product}'",
            order.GetType().Name, order.OrderId, order.Amount, order.Product);

        order.Process();

        Log.Information("Order {OrderId} complete", order.OrderId);
    }
}
