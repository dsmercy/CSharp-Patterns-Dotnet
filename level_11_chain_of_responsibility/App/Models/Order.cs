namespace Chain.App.Models;

public record Order(
    string  OrderId,
    string  CustomerId,
    decimal Amount,
    string  ShippingRegion,
    bool    HasValidPayment,
    bool    IsFraudulent);
