using AbstractFactory.App.Interfaces;

namespace AbstractFactory.App.Stripe;

// PATTERN CONCEPT: Concrete Factory A — every method returns a Stripe object.
// It is impossible to get a PayPal object out of this factory.
public class StripeFactory : IPaymentProcessorFactory
{
    public string ProviderName => "Stripe";

    public IPaymentProcessor CreatePaymentProcessor() => new StripePaymentProcessor();
    public IRefundProcessor  CreateRefundProcessor()  => new StripeRefundProcessor();
    public IWebhookHandler   CreateWebhookHandler()   => new StripeWebhookHandler();
}
