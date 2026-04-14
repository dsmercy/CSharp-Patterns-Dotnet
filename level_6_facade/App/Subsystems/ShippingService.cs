using Facade.App.Models;
using Serilog;

namespace Facade.App.Subsystems;

public record ShipmentConfirmation(string TrackingNumber, string Carrier, DateTime EstimatedDelivery);

public class ShippingService
{
    public async Task<ShipmentConfirmation> BookAsync(Address address, List<CartItem> items)
    {
        await Task.Delay(30); // simulate courier API
        var tracking  = $"TRK-{Guid.NewGuid():N}"[..12].ToUpper();
        var estimated = DateTime.UtcNow.AddDays(3);
        Log.Information("    [Shipping] Booked {Count} item(s) to {Address} — tracking: {Track}, ETA: {Eta:yyyy-MM-dd}",
            items.Count, address, tracking, estimated);
        return new ShipmentConfirmation(tracking, "RoyalMail", estimated);
    }
}
