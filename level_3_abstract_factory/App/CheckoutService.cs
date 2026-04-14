using AbstractFactory.App.Interfaces;
using Serilog;

namespace AbstractFactory.App;

// PATTERN CONCEPT: the Client.
//
// CheckoutService depends only on the IPaymentProcessorFactory interface — it has
// ZERO knowledge of Stripe, PayPal, or any other concrete provider.
//
// The factory is injected at construction time (constructor injection — the same
// mechanism used by ASP.NET Core DI). Swap the factory, swap the entire family.
// No conditional logic inside this class ever needs to change.
public class CheckoutService(IPaymentProcessorFactory factory)
{
    private readonly IPaymentProcessor _processor = factory.CreatePaymentProcessor();
    private readonly IRefundProcessor  _refunder  = factory.CreateRefundProcessor();
    private readonly IWebhookHandler   _webhook   = factory.CreateWebhookHandler();

    public string ProcessCheckout(Order order)
    {
        Log.Information("  [CheckoutService] Processing checkout via {Provider}", factory.ProviderName);

        var result = _processor.Charge(order.OrderId, order.Amount, order.Currency);

        if (!result.Success)
        {
            Log.Error("  [CheckoutService] Charge failed: {Message}", result.Message);
            return string.Empty;
        }

        Log.Information("  [CheckoutService] Charge succeeded — transaction: {TxnId}", result.TransactionId);
        return result.TransactionId;
    }

    public void ProcessRefund(string transactionId, decimal amount, string reason)
    {
        Log.Information("  [CheckoutService] Refunding {Amount} on txn {TxnId}", amount, transactionId);

        var result = _refunder.Refund(transactionId, amount, reason);

        if (result.Success)
            Log.Information("  [CheckoutService] Refund complete — refund ID: {RefundId}", result.RefundId);
        else
            Log.Error("  [CheckoutService] Refund failed: {Message}", result.Message);
    }

    public void HandleWebhook(string payload, string signature)
    {
        Log.Information("  [CheckoutService] Incoming webhook — verifying signature");

        if (!_webhook.VerifySignature(payload, signature))
        {
            Log.Error("  [CheckoutService] Webhook rejected — invalid signature");
            return;
        }

        var eventType = _webhook.HandleEvent(payload);
        Log.Information("  [CheckoutService] Webhook processed — event: {Event}", eventType);
    }
}

public record Order(string OrderId, string CustomerId, decimal Amount, string Currency = "GBP");
