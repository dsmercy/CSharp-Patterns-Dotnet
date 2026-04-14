// PROBLEM: BadCheckoutService hardcodes concrete payment provider types directly.
// Two problems demonstrated:
//   1. Mixed-family bug — StripePaymentProcessor paired with PayPalWebhookHandler
//      compiles fine but fails at runtime when a webhook arrives (wrong signature format).
//   2. Provider switch cost — every line that must change to move from Stripe to PayPal
//      is marked with ← CHANGE THIS.

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== PROBLEM: Hardcoded providers — mixed family causes runtime failure ===");
Log.Information("");

var order = new Order("ORD-001", "CUST-001", 149.99m);

// ─── Scenario 1: Mixed family — Stripe processor + PayPal webhook handler ─────
Log.Information("--- Scenario 1: Mixed-family wiring (Stripe charge + PayPal webhook) ---");
var badService = new BadCheckoutService();
badService.ProcessCheckout(order);

Log.Information("");
Log.Information("  Now a PayPal webhook arrives...");
badService.HandleWebhook(
    payload:   """{"event":"payment.completed","provider":"paypal"}""",
    signature: "paypal-hmac-sha256-abc123");   // PayPal signature format

Log.Information("");

// ─── Scenario 2: What changes when switching to PayPal? ──────────────────────
Log.Information("--- Scenario 2: Lines that MUST change to switch to PayPal ---");
Log.Information("  (see BadCheckoutService source — every ← CHANGE THIS comment)");
Log.Information("  Line 1: new StripePaymentProcessor()  →  new PayPalPaymentProcessor()");
Log.Information("  Line 2: new StripeRefundProcessor()   →  new PayPalRefundProcessor()");
Log.Information("  Line 3: new StripeWebhookHandler()    →  new PayPalWebhookHandler()");
Log.Information("  Miss one: mismatched family, runtime failure, no compiler warning.");
Log.Information("");
Log.Information(">>> Fix: Abstract Factory — one line in the composition root");
Log.Information(">>>      switches the ENTIRE family atomically.");

// ─── Bad service (inlines all provider choices) ───────────────────────────────

public class BadCheckoutService
{
    // PROBLEM: concrete types hardcoded — three separate places to update per provider swap
    private readonly StripePaymentProcessor _processor = new();      // ← CHANGE THIS
    private readonly StripeRefundProcessor  _refunder  = new();      // ← CHANGE THIS
    private readonly PayPalWebhookHandler   _webhook   = new();      // ← CHANGE THIS (but developer forgot!)

    public void ProcessCheckout(Order order)
    {
        Log.Information("  [BadCheckoutService] Charging £{Amount} via {Provider}",
            order.Amount, _processor.ProviderName);
        var result = _processor.Charge(order.OrderId, order.Amount);
        Log.Information("  [BadCheckoutService] Charge result: {Result}", result);
    }

    public void HandleWebhook(string payload, string signature)
    {
        // _webhook is PayPal but _processor is Stripe — mismatched family!
        // PayPal signatures start with "paypal-"; Stripe verifier expects "whsec_" format.
        var valid = _webhook.Verify(payload, signature);
        if (valid)
            Log.Information("  [BadCheckoutService] Webhook verified and processed");
        else
            Log.Error("  [BadCheckoutService] WEBHOOK VERIFICATION FAILED — " +
                      "handler is {Handler} but processor is {Proc}. Mismatched family!",
                      _webhook.ProviderName, _processor.ProviderName);
    }
}

// ─── Stub providers (inlined for self-contained demo) ────────────────────────

public class StripePaymentProcessor
{
    public string ProviderName => "Stripe";
    public string Charge(string orderId, decimal amount) =>
        $"stripe_txn_{orderId}_{amount:F0}";
}

public class StripeRefundProcessor
{
    public string ProviderName => "Stripe";
    public string Refund(string txnId, decimal amount) =>
        $"stripe_ref_{txnId}_{amount:F0}";
}

public class PayPalWebhookHandler
{
    public string ProviderName => "PayPal";

    // PayPal expects "paypal-hmac-sha256-" prefix; Stripe sends "whsec_" prefix.
    public bool Verify(string payload, string signature) =>
        signature.StartsWith("paypal-hmac-sha256-");
}

public record Order(string OrderId, string CustomerId, decimal Amount);
