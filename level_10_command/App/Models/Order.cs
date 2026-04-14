namespace Command.App.Models;

public class Order
{
    public string  OrderId    { get; }
    public string  CustomerId { get; }
    public decimal Amount     { get; set; }
    public string  Status     { get; set; } = "New";

    public Order(string orderId, string customerId, decimal amount)
    {
        OrderId    = orderId;
        CustomerId = customerId;
        Amount     = amount;
    }

    public override string ToString() =>
        $"Order({OrderId}, £{Amount:F2}, {Status})";
}
