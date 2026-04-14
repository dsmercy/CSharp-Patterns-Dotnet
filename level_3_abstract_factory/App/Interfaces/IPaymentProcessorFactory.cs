namespace AbstractFactory.App.Interfaces;

// PATTERN CONCEPT: the Abstract Factory.
// Declares one Create* method per product in the family.
// Concrete factories (StripeFactory, PayPalFactory) implement all three methods,
// guaranteeing that every returned object belongs to the same provider family.
public interface IPaymentProcessorFactory
{
    IPaymentProcessor CreatePaymentProcessor();
    IRefundProcessor  CreateRefundProcessor();
    IWebhookHandler   CreateWebhookHandler();

    // Human-readable label for logging / display.
    string ProviderName { get; }
}
