using Serilog;

namespace Adapter.App.Legacy;

// PATTERN CONCEPT: the Adaptee.
//
// This simulates an unmodifiable third-party library (delivered as a NuGet package
// or a DLL you do not own). The XML-based method signatures are fixed — you cannot
// add new overloads, rename methods, or change return types.
//
// In a real project this class would live in a referenced assembly, not your codebase.
public class LegacyPaymentGateway
{
    // Old XML-based charge API.
    // Returns an XML string with <Status>, <TxnRef>, <AuthCode>, <ErrorCode>.
    public string ProcessXmlPayment(string xmlPayload)
    {
        var amount   = ExtractXmlValue(xmlPayload, "Amount");
        var currency = ExtractXmlValue(xmlPayload, "Currency");
        var orderId  = ExtractXmlValue(xmlPayload, "OrderRef");

        Log.Information("    [LegacyGateway] ProcessXmlPayment — order: {Order}, {Amount} {Ccy}",
            orderId, amount, currency);

        return $"""
            <Response>
              <Status>APPROVED</Status>
              <TxnRef>TXN-{orderId}-{DateTime.UtcNow:mmss}</TxnRef>
              <AuthCode>AUTH-{Guid.NewGuid():N}</AuthCode>
              <ErrorCode></ErrorCode>
            </Response>
            """;
    }

    // Old XML-based refund API.
    // Returns an XML string with <Status>, <RefundRef>, <ErrorCode>.
    public string ReverseTransaction(string txnRef, string xmlPayload)
    {
        var amount = ExtractXmlValue(xmlPayload, "Amount");
        Log.Information("    [LegacyGateway] ReverseTransaction — txnRef: {TxnRef}, amount: {Amount}",
            txnRef, amount);

        return $"""
            <Response>
              <Status>REVERSED</Status>
              <RefundRef>REF-{txnRef[^Math.Min(6, txnRef.Length)..]}</RefundRef>
              <ErrorCode></ErrorCode>
            </Response>
            """;
    }

    internal static string ExtractXmlValue(string xml, string tag)
    {
        var open  = $"<{tag}>";
        var close = $"</{tag}>";
        var start = xml.IndexOf(open,  StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        start += open.Length;
        var end = xml.IndexOf(close, start, StringComparison.Ordinal);
        return end < 0 ? string.Empty : xml[start..end].Trim();
    }
}
