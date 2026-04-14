using AbstractFactory.App.Interfaces;
using Serilog;

namespace AbstractFactory.App.PayPal;

public class PayPalRefundProcessor : IRefundProcessor
{
    public RefundResult Refund(string transactionId, decimal amount, string reason)
    {
        // PayPal refund IDs are prefixed "REF-"
        var refundId = $"REF-{transactionId[..6]}-{Guid.NewGuid():N}"[..20].ToUpper();
        Log.Information("    [PayPal] Issuing refund of {Amount} against {TxnId} — reason: {Reason}",
            amount, transactionId, reason);
        Log.Information("    [PayPal] Refund ID: {RefundId}", refundId);
        return new RefundResult(true, refundId, "Refund processed");
    }
}
