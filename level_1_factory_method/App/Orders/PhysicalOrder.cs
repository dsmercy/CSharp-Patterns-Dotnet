using Serilog;

namespace FactoryMethod.App.Orders;

// Concrete Product: requires a shipping address and generates a courier label.
public class PhysicalOrder : Order
{
    public string TrackingNumber { get; }

    public PhysicalOrder(string customerId, string product, decimal amount)
        : base(customerId, product, amount)
    {
        TrackingNumber = $"TRK-{Guid.NewGuid():N}"[..12].ToUpper();
    }

    public override void Process()
    {
        Log.Information("  [PhysicalOrder] Checking inventory for '{Product}'", Product);
        Log.Information("  [PhysicalOrder] Generating shipping label — tracking: {Tracking}", TrackingNumber);
        Log.Information("  [PhysicalOrder] Dispatching to courier for customer {CustomerId}", CustomerId);
    }
}
