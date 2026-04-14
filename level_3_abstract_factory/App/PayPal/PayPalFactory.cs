using AbstractFactory.App.Interfaces;

namespace AbstractFactory.App.PayPal;

// PATTERN CONCEPT: Concrete Factory B — every method returns a PayPal object.
// CheckoutService never changes; swapping StripeFactory for PayPalFactory here
// switches the entire payment family atomically.
public class PayPalFactory : IPaymentProcessorFactory
{
    public string ProviderName => "PayPal";

    public IPaymentProcessor CreatePaymentProcessor() => new PayPalPaymentProcessor();
    public IRefundProcessor  CreateRefundProcessor()  => new PayPalRefundProcessor();
    public IWebhookHandler   CreateWebhookHandler()   => new PayPalWebhookHandler();
}
