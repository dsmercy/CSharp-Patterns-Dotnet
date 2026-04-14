using Adapter.App.Legacy;
using Adapter.App.Modern;
using Serilog;
using static Adapter.App.Legacy.LegacyPaymentGateway;

namespace Adapter.App;

// PATTERN CONCEPT: the Adapter (Object Adapter via composition).
//
// Implements the Target interface (IPaymentGateway) so the rest of the system
// can use it without knowing anything about the legacy XML protocol.
//
// Wraps the Adaptee (LegacyPaymentGateway) via a private field — this is the
// Object Adapter approach, preferred in C# over Class Adapter (inheritance) because:
//   - C# does not support multiple class inheritance.
//   - Composition is more flexible: you can swap the wrapped instance at runtime.
//
// All XML construction, response parsing, and legacy error-code mapping live here —
// in ONE place, tested once, invisible to every call site.
public class LegacyPaymentGatewayAdapter(LegacyPaymentGateway legacy) : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(string orderId, decimal amount, string currency)
    {
        Log.Information("  [Adapter] Translating ChargeAsync → ProcessXmlPayment");

        // Translate: modern parameters → legacy XML request
        var xml = $"""
            <PaymentRequest>
              <OrderRef>{orderId}</OrderRef>
              <Amount>{amount:F2}</Amount>
              <Currency>{currency}</Currency>
              <MerchantId>MERCHANT-001</MerchantId>
            </PaymentRequest>
            """;

        var response = legacy.ProcessXmlPayment(xml);

        // Translate: legacy XML response → modern PaymentResult
        var status    = LegacyPaymentGateway.ExtractXmlValue(response, "Status");
        var txnRef    = LegacyPaymentGateway.ExtractXmlValue(response, "TxnRef");
        var authCode  = LegacyPaymentGateway.ExtractXmlValue(response, "AuthCode");
        var errorCode = LegacyPaymentGateway.ExtractXmlValue(response, "ErrorCode");

        var result = new PaymentResult(
            Success:       status == "APPROVED",
            TransactionId: txnRef,
            AuthCode:      authCode,
            ErrorCode:     errorCode);

        Log.Information("  [Adapter] ChargeAsync result: {Success} — txn: {TxnId}",
            result.Success, result.TransactionId);

        return Task.FromResult(result);
    }

    public Task<RefundResult> RefundAsync(string transactionId, decimal amount, string reason)
    {
        Log.Information("  [Adapter] Translating RefundAsync → ReverseTransaction");

        // Translate: modern parameters → legacy XML request
        var xml = $"""
            <RefundRequest>
              <Amount>{amount:F2}</Amount>
              <Reason>{reason}</Reason>
            </RefundRequest>
            """;

        var response  = legacy.ReverseTransaction(transactionId, xml);

        // Translate: legacy XML response → modern RefundResult
        var status    = LegacyPaymentGateway.ExtractXmlValue(response, "Status");
        var refundRef = LegacyPaymentGateway.ExtractXmlValue(response, "RefundRef");
        var errorCode = LegacyPaymentGateway.ExtractXmlValue(response, "ErrorCode");

        var result = new RefundResult(
            Success:  status == "REVERSED",
            RefundId: refundRef,
            ErrorCode: errorCode);

        Log.Information("  [Adapter] RefundAsync result: {Success} — refund: {RefundId}",
            result.Success, result.RefundId);

        return Task.FromResult(result);
    }
}
