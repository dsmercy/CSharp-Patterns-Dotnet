namespace Adapter.App.Models;

public record Order(string OrderId, string CustomerId, decimal Amount, string Currency = "GBP");
