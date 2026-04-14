using Serilog;

namespace Facade.App.Subsystems;

public record PaymentConfirmation(string TransactionId, decimal AmountCharged);

public class PaymentService
{
    public async Task<PaymentConfirmation> ChargeAsync(string customerId, decimal amount)
    {
        await Task.Delay(40); // simulate payment provider API
        var txnId = $"TXN-{Guid.NewGuid():N}"[..14].ToUpper();
        Log.Information("    [Payment] Charged £{Amount:F2} for customer {Id} — txn: {Txn}",
            amount, customerId, txnId);
        return new PaymentConfirmation(txnId, amount);
    }
}
