namespace Builder.App.Models;

// PATTERN CONCEPT: this is the "Product" — the complex object being constructed.
// It has no public constructor; the only way to create a valid Order is through
// OrderBuilder.Build(), which enforces all validation in one place.
public class Order
{
    public string         OrderId         { get; } = $"ORD-{Guid.NewGuid():N}"[..8].ToUpper();
    public string         CustomerId      { get; internal set; } = string.Empty;
    public List<OrderItem> Items          { get; } = [];
    public Address?       ShippingAddress { get; internal set; }
    public Address?       BillingAddress  { get; internal set; }
    public string?        DiscountCode    { get; internal set; }
    public decimal        DiscountPercent { get; internal set; }
    public string?        GiftMessage     { get; internal set; }
    public string?        DeliveryWindow  { get; internal set; }
    public string         PaymentMethod   { get; internal set; } = "CreditCard";
    public bool           IsRecurring     { get; internal set; }
    public int            LoyaltyPoints   { get; internal set; }

    // Calculated by Build() — not settable by callers.
    public decimal Amount { get; internal set; }

    // Internal constructor — only OrderBuilder can create instances.
    internal Order() { }
}
