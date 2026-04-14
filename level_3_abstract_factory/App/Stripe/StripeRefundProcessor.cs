using AbstractFactory.App.Interfaces;
using Serilog;

namespace AbstractFactory.App.Stripe;

public class StripeRefundProcessor : IRefundProcessor
{
    public RefundResult Refund(string transactionId, decimal amount, string reason)
    {
        // Stripe refund IDs are prefixed "re_"
        var refundId = $"re_{transactionId[..6]}_{Guid.NewGuid():N}"[..18];
        Log.Information("    [Stripe] Issuing refund of {Amount} against {TxnId} — reason: {Reason}",
            amount, transactionId, reason);
        Log.Information("    [Stripe] Refund ID: {RefundId}", refundId);
        return new RefundResult(true, refundId, "Refund issued");
    }
}
