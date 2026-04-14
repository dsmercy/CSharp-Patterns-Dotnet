using Adapter.App.Models;
using Adapter.App.Modern;
using Serilog;

namespace Adapter.App;

// The client — depends only on IPaymentGateway (the Target interface).
// It has no knowledge of XML, legacy error codes, or the LegacyPaymentGateway class.
// Swap LegacyPaymentGatewayAdapter for a ModernPaymentGateway and this class
// compiles and runs without a single line change.
public class CheckoutService(IPaymentGateway gateway)
{
    public async Task ProcessCheckout(Order order)
    {
        Log.Information("  [CheckoutService] Processing checkout for {OrderId}", order.OrderId);

        var result = await gateway.ChargeAsync(order.OrderId, order.Amount, order.Currency);

        if (result.Success)
            Log.Information("  [CheckoutService] Checkout OK — txn: {TxnId}, auth: {Auth}",
                result.TransactionId, result.AuthCode);
        else
            Log.Error("  [CheckoutService] Checkout FAILED — error: {Code}", result.ErrorCode);
    }

    public async Task ProcessRefund(string transactionId, decimal amount, string reason)
    {
        Log.Information("  [CheckoutService] Refunding {Amount} on txn {TxnId}", amount, transactionId);

        var result = await gateway.RefundAsync(transactionId, amount, reason);

        if (result.Success)
            Log.Information("  [CheckoutService] Refund OK — refund ID: {RefundId}", result.RefundId);
        else
            Log.Error("  [CheckoutService] Refund FAILED — error: {Code}", result.ErrorCode);
    }
}
