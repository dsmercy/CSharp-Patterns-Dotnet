using AbstractFactory.App.Interfaces;
using Serilog;

namespace AbstractFactory.App.Stripe;

public class StripeWebhookHandler : IWebhookHandler
{
    // Stripe webhook signatures are prefixed "whsec_"
    public bool VerifySignature(string payload, string signature)
    {
        var valid = signature.StartsWith("whsec_");
        Log.Information("    [Stripe] Verifying webhook signature — format valid: {Valid}", valid);
        return valid;
    }

    public string HandleEvent(string payload)
    {
        Log.Information("    [Stripe] Parsing Stripe event from payload");
        // In production: deserialise the Stripe Event object and dispatch by event type.
        return "stripe.payment_intent.succeeded";
    }
}
