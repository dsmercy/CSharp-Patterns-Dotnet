// SOLUTION: Abstract Factory — the entire payment provider family is selected once
// in the composition root. CheckoutService never changes regardless of provider.

using AbstractFactory.App;
using AbstractFactory.App.Interfaces;
using AbstractFactory.App.PayPal;
using AbstractFactory.App.Stripe;
using Microsoft.Extensions.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var providerName = config["Demo:PaymentProvider"]
    ?? throw new InvalidOperationException("Demo:PaymentProvider not set in appsettings.json");

// PATTERN CONCEPT: ONE switch in the composition root selects the entire family.
// Changing "Stripe" to "PayPal" in appsettings.json switches all three products atomically.
// It is impossible for CheckoutService to end up with a mixed-family configuration.
IPaymentProcessorFactory factory = providerName switch
{
    "Stripe" => new StripeFactory(),
    "PayPal" => new PayPalFactory(),
    _ => throw new InvalidOperationException(
             $"Unknown provider '{providerName}'. Valid values: Stripe, PayPal")
};

Log.Information("=== Abstract Factory Pattern — Payment Provider: {Provider} ===", factory.ProviderName);
Log.Information("");

// CheckoutService receives the factory — it never knows or cares which provider is active.
var checkout = new CheckoutService(factory);
var order    = new Order("ORD-001", "CUST-001", 149.99m, "GBP");

// ─── Scenario 1: Checkout ────────────────────────────────────────────────────
Log.Information("--- Scenario 1: Process checkout ---");
var txnId = checkout.ProcessCheckout(order);
Log.Information("");

// ─── Scenario 2: Refund ──────────────────────────────────────────────────────
Log.Information("--- Scenario 2: Partial refund ---");
checkout.ProcessRefund(txnId, 49.99m, "Customer returned one item");
Log.Information("");

// ─── Scenario 3: Webhook (matching provider — succeeds) ──────────────────────
Log.Information("--- Scenario 3: Webhook from matching provider (should succeed) ---");
var (validSig, validPayload) = providerName switch
{
    "Stripe" => ("whsec_live_abc123xyz",              """{"type":"payment_intent.succeeded"}"""),
    "PayPal" => ("paypal-hmac-sha256-def456uvw",      """{"event_type":"PAYMENT.CAPTURE.COMPLETED"}"""),
    _        => ("", "")
};
checkout.HandleWebhook(validPayload, validSig);
Log.Information("");

// ─── Scenario 4: Webhook from WRONG provider (should fail) ───────────────────
Log.Information("--- Scenario 4: Webhook from wrong provider (mismatched signature — should fail) ---");
var wrongSig = providerName switch
{
    "Stripe" => "paypal-hmac-sha256-wrong",   // PayPal sig sent to Stripe handler
    "PayPal" => "whsec_wrong",                // Stripe sig sent to PayPal handler
    _        => ""
};
checkout.HandleWebhook(validPayload, wrongSig);
Log.Information("");

Log.Information("=== Key point ===");
Log.Information("  CheckoutService contains zero provider conditionals.");
Log.Information("  Change Demo:PaymentProvider in appsettings.json to switch the entire family.");
Log.Information("  Adding a new provider (e.g. Square) = one new SquareFactory class,");
Log.Information("  zero changes to CheckoutService or any existing factory.");
