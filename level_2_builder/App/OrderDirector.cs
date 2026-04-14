using Builder.App.Models;

namespace Builder.App;

// PATTERN CONCEPT: the Director.
//
// The Director is optional. It encodes pre-defined, reusable construction sequences
// that are always built the same way. It does NOT create the builder — the caller
// passes one in, so the caller still controls the final Build() call and can add
// extra steps before or after the director runs.
//
// Use the Director when: several callers always need the same set of builder steps
// and you want one canonical place to keep that sequence.
public class OrderDirector
{
    // Always adds a gift message, express delivery, and a gift-wrapping item.
    public void BuildGiftOrder(OrderBuilder builder, string customerId, string giftMessage)
    {
        builder
            .WithCustomer(customerId)
            .WithItem("SKU-WRAP", "Premium Gift Wrapping", 1, 4.99m)
            .WithGiftMessage(giftMessage)
            .WithDeliveryWindow("Next Day")
            .WithPaymentMethod("CreditCard");
    }

    // Always sets up recurring billing and marks the order as a subscription.
    public void BuildSubscriptionOrder(OrderBuilder builder, string customerId, string planName, decimal monthlyPrice)
    {
        builder
            .WithCustomer(customerId)
            .WithItem("SUB-PLAN", planName, 1, monthlyPrice)
            .AsRecurring()
            .WithPaymentMethod("DirectDebit");
    }
}
