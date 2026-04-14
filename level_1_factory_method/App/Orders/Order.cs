namespace FactoryMethod.App.Orders;

// PATTERN CONCEPT: this is the "Product" in Factory Method terminology.
// The abstract base defines the interface that all order types must honour.
// The factory method returns this type — callers never depend on the concrete subclass.
public abstract class Order
{
    public string  OrderId    { get; } = $"ORD-{Guid.NewGuid():N}"[..10].ToUpper();
    public string  CustomerId { get; }
    public string  Product    { get; }
    public decimal Amount     { get; }

    protected Order(string customerId, string product, decimal amount)
    {
        CustomerId = customerId;
        Product    = product;
        Amount     = amount;
    }

    // Each concrete order type defines its own processing rules here.
    public abstract void Process();
}
