// SOLUTION: Adapter pattern — LegacyPaymentGatewayAdapter wraps the XML-based
// LegacyPaymentGateway behind IPaymentGateway. CheckoutService uses only the
// modern interface and is completely unaware of the legacy protocol.

using Adapter.App;
using Adapter.App.Legacy;
using Adapter.App.Models;
using Adapter.App.Modern;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== Adapter Pattern — Legacy Payment Gateway Demo ===");
Log.Information("");

// PATTERN CONCEPT: the composition root wires the Adapter.
// CheckoutService only ever sees IPaymentGateway — the XML detail is invisible.
IPaymentGateway gateway = new LegacyPaymentGatewayAdapter(new LegacyPaymentGateway());
var checkout            = new CheckoutService(gateway);
var order               = new Order("ORD-001", "CUST-001", 149.99m, "GBP");

// ─── Scenario 1: Charge ───────────────────────────────────────────────────────
Log.Information("--- Scenario 1: Checkout (CheckoutService calls ChargeAsync) ---");
await checkout.ProcessCheckout(order);
Log.Information("");

// ─── Scenario 2: Refund ───────────────────────────────────────────────────────
Log.Information("--- Scenario 2: Refund (CheckoutService calls RefundAsync) ---");
await checkout.ProcessRefund("TXN-ORD-001-4523", 49.99m, "CustomerReturn");
Log.Information("");

// ─── Scenario 3: Swappability — show that CheckoutService is unchanged ────────
Log.Information("--- Scenario 3: Swapping the gateway (adapter → stub) ---");
Log.Information("  Replace LegacyPaymentGatewayAdapter with StubPaymentGateway below.");
Log.Information("  CheckoutService source code: ZERO changes.");
Log.Information("");

IPaymentGateway stub = new StubPaymentGateway();
var checkoutWithStub  = new CheckoutService(stub);
await checkoutWithStub.ProcessCheckout(new Order("ORD-002", "CUST-002", 29.99m));
Log.Information("");

Log.Information("=== Key point ===");
Log.Information("  XML construction and response parsing live in ONE place: the Adapter.");
Log.Information("  All three call sites in Problem/Program.cs collapse to one class.");
Log.Information("  Replacing the gateway later = swap one line in the composition root.");

// ─── Stub — demonstrates the Target interface is the only coupling ─────────────

public class StubPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(string orderId, decimal amount, string currency)
    {
        Log.Information("    [StubGateway] ChargeAsync — simulating instant approval");
        return Task.FromResult(new PaymentResult(true, $"STUB-TXN-{orderId}", "STUB-AUTH", string.Empty));
    }

    public Task<RefundResult> RefundAsync(string transactionId, decimal amount, string reason)
    {
        Log.Information("    [StubGateway] RefundAsync — simulating instant refund");
        return Task.FromResult(new RefundResult(true, $"STUB-REF-{transactionId[^4..]}", string.Empty));
    }
}
