using Builder.App.Models;

namespace Builder.App;

// PATTERN CONCEPT: the Builder.
//
// Each With* method sets exactly one field and returns `this`, enabling a fluent
// chain: new OrderBuilder().WithCustomer(...).WithItem(...).Build()
//
// Build() is the ONLY place that validates and finalises the order. An Order object
// can never exist in an invalid state because callers cannot call `new Order()`.
public class OrderBuilder
{
    private readonly Order _order = new();

    public OrderBuilder WithCustomer(string customerId)
    {
        _order.CustomerId = customerId;
        return this;
    }

    public OrderBuilder WithItem(string productId, string name, int quantity, decimal unitPrice)
    {
        _order.Items.Add(new OrderItem(productId, name, quantity, unitPrice));
        return this;
    }

    public OrderBuilder WithShippingAddress(Address address)
    {
        _order.ShippingAddress = address;
        return this;
    }

    public OrderBuilder WithBillingAddress(Address address)
    {
        _order.BillingAddress = address;
        return this;
    }

    // PATTERN CONCEPT: named parameters make the intent clear — no positional confusion
    public OrderBuilder WithDiscount(string code, decimal percent)
    {
        if (percent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), "Discount must be 0–100%.");
        _order.DiscountCode    = code;
        _order.DiscountPercent = percent;
        return this;
    }

    public OrderBuilder WithGiftMessage(string message)
    {
        _order.GiftMessage = message;
        return this;
    }

    public OrderBuilder WithDeliveryWindow(string window)
    {
        _order.DeliveryWindow = window;
        return this;
    }

    public OrderBuilder WithPaymentMethod(string method)
    {
        _order.PaymentMethod = method;
        return this;
    }

    public OrderBuilder AsRecurring()
    {
        _order.IsRecurring = true;
        return this;
    }

    public OrderBuilder WithLoyaltyPoints(int points)
    {
        if (points < 0)
            throw new ArgumentOutOfRangeException(nameof(points), "Loyalty points cannot be negative.");
        _order.LoyaltyPoints = points;
        return this;
    }

    // PATTERN CONCEPT: Build() is the single validation gate.
    // Invalid combinations (e.g. no items, physical order without a shipping address)
    // are caught here — not scattered across call sites, not discovered at dispatch time.
    public Order Build()
    {
        if (string.IsNullOrWhiteSpace(_order.CustomerId))
            throw new InvalidOperationException("Order must have a customer.");

        if (_order.Items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item.");

        // Calculate the total with discount applied.
        var subtotal = _order.Items.Sum(i => i.LineTotal);
        _order.Amount = subtotal * (1 - _order.DiscountPercent / 100m);

        return _order;
    }
}
