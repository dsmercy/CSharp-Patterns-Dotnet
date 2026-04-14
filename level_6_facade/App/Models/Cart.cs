namespace Facade.App.Models;

public record CartItem(string Sku, string Name, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}

public record Cart(
    string         CustomerId,
    List<CartItem> Items,
    Address        ShippingAddress)
{
    public decimal Total => Items.Sum(i => i.LineTotal);
}
