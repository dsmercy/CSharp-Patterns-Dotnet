// PROBLEM: BadCheckoutService calls LegacyPaymentGateway.ProcessXmlPayment directly.
// Two problems demonstrated:
//   1. XML construction, response parsing, and legacy error-code mapping are all
//      inlined — and duplicated across two call sites (checkout + retry logic).
//   2. The business code is coupled to the legacy API format. Replacing the gateway
//      later means touching every call site, not just one adapter class.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: Legacy gateway coupled directly into business logic ===");
Log.Information("");

var gateway = new LegacyPaymentGateway();
var service = new BadCheckoutService(gateway);

Log.Information("--- Scenario 1: Normal checkout ---");
service.ProcessCheckout("ORD-001", 149.99m, "GBP");

Log.Information("");
Log.Information("--- Scenario 2: Checkout with retry (XML logic duplicated again) ---");
service.ProcessCheckoutWithRetry("ORD-002", 89.99m, "GBP");

Log.Information("");
Log.Information("--- Scenario 3: Refund (yet more XML construction + parsing) ---");
service.ProcessRefund("TXN-ABC123", 149.99m);

Log.Information("");
Log.Information(">>> Problems:");
Log.Information("  1. XML building/parsing is duplicated across ProcessCheckout,");
Log.Information("     ProcessCheckoutWithRetry, and ProcessRefund.");
Log.Information("  2. Legacy error codes (ERR_INSUF_FUNDS, ERR_DECLINED) are mapped");
Log.Information("     in EACH call site — update one, forget the others.");
Log.Information("  3. Replacing the gateway = touching every inlined call site.");
Log.Information(">>> Fix: Adapter wraps LegacyPaymentGateway once behind IPaymentGateway.");

// ─── Legacy gateway (simulates an unmodifiable third-party library) ───────────

public class LegacyPaymentGateway
{
    // Old XML-based API — method signatures cannot be changed (third-party binary).
    public string ProcessXmlPayment(string xmlPayload)
    {
        // Parse the XML request (simplified — real gateway would validate schema).
        var amount   = ExtractXmlValue(xmlPayload, "Amount");
        var currency = ExtractXmlValue(xmlPayload, "Currency");
        var orderId  = ExtractXmlValue(xmlPayload, "OrderRef");

        Log.Information("  [LegacyGateway] ProcessXmlPayment called — order: {Order}, amount: {Amount} {Ccy}",
            orderId, amount, currency);

        // Simulate success — return legacy XML response.
        return $"""
            <Response>
              <Status>APPROVED</Status>
              <TxnRef>TXN-{orderId}-{DateTime.UtcNow:mmss}</TxnRef>
              <AuthCode>AUTH-{Guid.NewGuid():N}"[..6].ToUpper()</AuthCode>
              <ErrorCode></ErrorCode>
            </Response>
            """;
    }

    public string ReverseTransaction(string txnRef, string xmlPayload)
    {
        var amount = ExtractXmlValue(xmlPayload, "Amount");
        Log.Information("  [LegacyGateway] ReverseTransaction called — txnRef: {TxnRef}, amount: {Amount}",
            txnRef, amount);

        return $"""
            <Response>
              <Status>REVERSED</Status>
              <RefundRef>REF-{txnRef[^6..]}</RefundRef>
              <ErrorCode></ErrorCode>
            </Response>
            """;
    }

    private static string ExtractXmlValue(string xml, string tag)
    {
        var open  = $"<{tag}>";
        var close = $"</{tag}>";
        var start = xml.IndexOf(open, StringComparison.Ordinal);
        if (start < 0) return "?";
        start += open.Length;
        var end = xml.IndexOf(close, start, StringComparison.Ordinal);
        return end < 0 ? "?" : xml[start..end];
    }
}

// ─── Bad service: every call site builds/parses XML itself ───────────────────

public class BadCheckoutService(LegacyPaymentGateway gateway)
{
    public void ProcessCheckout(string orderId, decimal amount, string currency)
    {
        // PROBLEM: XML construction inlined — first call site
        var xml = $"""
            <PaymentRequest>
              <OrderRef>{orderId}</OrderRef>
              <Amount>{amount:F2}</Amount>
              <Currency>{currency}</Currency>
              <MerchantId>MERCHANT-001</MerchantId>
            </PaymentRequest>
            """;

        var response = gateway.ProcessXmlPayment(xml);

        // PROBLEM: XML parsing + error-code mapping inlined — first call site
        var status    = ExtractXmlValue(response, "Status");
        var txnRef    = ExtractXmlValue(response, "TxnRef");
        var errorCode = ExtractXmlValue(response, "ErrorCode");

        if (status == "APPROVED")
            Log.Information("  [BadCheckoutService] Checkout OK — txn: {TxnRef}", txnRef);
        else
            Log.Error("  [BadCheckoutService] Checkout FAILED — legacy error: {Code}", errorCode);
    }

    public void ProcessCheckoutWithRetry(string orderId, decimal amount, string currency)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            // PROBLEM: identical XML construction duplicated — second call site
            var xml = $"""
                <PaymentRequest>
                  <OrderRef>{orderId}-ATT{attempt}</OrderRef>
                  <Amount>{amount:F2}</Amount>
                  <Currency>{currency}</Currency>
                  <MerchantId>MERCHANT-001</MerchantId>
                </PaymentRequest>
                """;

            var response = gateway.ProcessXmlPayment(xml);

            // PROBLEM: identical XML parsing duplicated — second call site
            var status    = ExtractXmlValue(response, "Status");
            var txnRef    = ExtractXmlValue(response, "TxnRef");
            var errorCode = ExtractXmlValue(response, "ErrorCode");

            if (status == "APPROVED")
            {
                Log.Information("  [BadCheckoutService] Retry attempt {A} succeeded — txn: {TxnRef}",
                    attempt, txnRef);
                return;
            }

            Log.Warning("  [BadCheckoutService] Attempt {A} failed — code: {Code}", attempt, errorCode);
        }
    }

    public void ProcessRefund(string txnRef, decimal amount)
    {
        // PROBLEM: yet more XML construction — third call site
        var xml = $"""
            <RefundRequest>
              <Amount>{amount:F2}</Amount>
              <Reason>CustomerReturn</Reason>
            </RefundRequest>
            """;

        var response  = gateway.ReverseTransaction(txnRef, xml);

        // PROBLEM: yet more XML parsing — third call site
        var status    = ExtractXmlValue(response, "Status");
        var refundRef = ExtractXmlValue(response, "RefundRef");
        var errorCode = ExtractXmlValue(response, "ErrorCode");

        if (status == "REVERSED")
            Log.Information("  [BadCheckoutService] Refund OK — refund ref: {RefundRef}", refundRef);
        else
            Log.Error("  [BadCheckoutService] Refund FAILED — legacy error: {Code}", errorCode);
    }

    private static string ExtractXmlValue(string xml, string tag)
    {
        var open  = $"<{tag}>";
        var close = $"</{tag}>";
        var start = xml.IndexOf(open, StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        start += open.Length;
        var end = xml.IndexOf(close, start, StringComparison.Ordinal);
        return end < 0 ? string.Empty : xml[start..end].Trim();
    }
}
