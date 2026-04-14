using AbstractFactory.App.Interfaces;
using Serilog;

namespace AbstractFactory.App.PayPal;

public class PayPalPaymentProcessor : IPaymentProcessor
{
    public ChargeResult Charge(string orderId, decimal amount, string currency)
    {
        // PayPal transaction IDs are prefixed "PAY-"
        var txnId = $"PAY-{orderId}-{Guid.NewGuid():N}"[..22].ToUpper();
        Log.Information("    [PayPal] Charging {Amount} {Currency} via Orders API v2 — txn: {TxnId}",
            amount, currency, txnId);
        return new ChargeResult(true, txnId, "Payment completed");
    }
}
