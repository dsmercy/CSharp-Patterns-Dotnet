using AbstractFactory.App.Interfaces;
using Serilog;

namespace AbstractFactory.App.Stripe;

public class StripePaymentProcessor : IPaymentProcessor
{
    public ChargeResult Charge(string orderId, decimal amount, string currency)
    {
        // Stripe transaction IDs are prefixed "ch_"
        var txnId = $"ch_{orderId}_{Guid.NewGuid():N}"[..20];
        Log.Information("    [Stripe] Charging {Amount} {Currency} via Payment Intents API — txn: {TxnId}",
            amount, currency, txnId);
        return new ChargeResult(true, txnId, "Payment succeeded");
    }
}
