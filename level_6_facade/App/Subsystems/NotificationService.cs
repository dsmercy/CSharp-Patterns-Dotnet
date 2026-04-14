using Serilog;

namespace Facade.App.Subsystems;

public class NotificationService
{
    public async Task SendConfirmationAsync(
        string customerId,
        PaymentConfirmation  payment,
        ShipmentConfirmation shipment)
    {
        await Task.Delay(15); // simulate email/SMS provider
        Log.Information("    [Notification] Confirmation sent to {CustomerId} — txn: {Txn}, tracking: {Track}",
            customerId, payment.TransactionId, shipment.TrackingNumber);
    }
}
