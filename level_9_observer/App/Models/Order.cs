namespace Observer.App.Models;

public record Order(string OrderId, string CustomerId, decimal Amount, string[] Items);
