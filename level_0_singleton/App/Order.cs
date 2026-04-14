namespace Singleton.App;

public class Order
{
    public string OrderId { get; }
    public string CustomerId { get; }
    public decimal Amount { get; }

    public Order(string orderId, string customerId, decimal amount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
    }
}
