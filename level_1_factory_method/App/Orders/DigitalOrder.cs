using Serilog;

namespace FactoryMethod.App.Orders;

// Concrete Product: instant delivery, no shipping label required.
public class DigitalOrder : Order
{
    public string DownloadUrl { get; }

    public DigitalOrder(string customerId, string product, decimal amount)
        : base(customerId, product, amount)
    {
        DownloadUrl = $"https://downloads.example.com/{OrderId}";
    }

    public override void Process()
    {
        Log.Information("  [DigitalOrder] Generating download link: {Url}", DownloadUrl);
        Log.Information("  [DigitalOrder] Sending download email to customer {CustomerId}", CustomerId);
        Log.Information("  [DigitalOrder] No shipping required — fulfilled instantly");
    }
}
