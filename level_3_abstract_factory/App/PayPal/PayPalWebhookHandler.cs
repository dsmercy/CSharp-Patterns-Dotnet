using AbstractFactory.App.Interfaces;
using Serilog;

namespace AbstractFactory.App.PayPal;

public class PayPalWebhookHandler : IWebhookHandler
{
    // PayPal webhook signatures are prefixed "paypal-hmac-sha256-"
    public bool VerifySignature(string payload, string signature)
    {
        var valid = signature.StartsWith("paypal-hmac-sha256-");
        Log.Information("    [PayPal] Verifying webhook signature — format valid: {Valid}", valid);
        return valid;
    }

    public string HandleEvent(string payload)
    {
        Log.Information("    [PayPal] Parsing PayPal IPN/webhook event from payload");
        // In production: deserialise the PayPal event type and dispatch accordingly.
        return "PAYMENT.CAPTURE.COMPLETED";
    }
}
