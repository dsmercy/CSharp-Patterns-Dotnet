namespace Builder.App.Models;

public record OrderItem(string ProductId, string Name, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}
