using Chain.App.Models;
using Serilog;

namespace Chain.App.Validators;

// PATTERN CONCEPT: Concrete Handler — validates payment credentials.
// Knows nothing about stock, fraud, or address — fully isolated.
public class PaymentValidator : OrderValidator
{
    public override async Task<ValidationResult> ValidateAsync(Order order)
    {
        await Task.Delay(30); // simulate payment gateway ping

        if (!order.HasValidPayment)
        {
            Log.Warning("  [PaymentValidator] FAIL — payment credentials invalid");
            return ValidationResult.Fail(
                "PAYMENT_INVALID", "Payment credentials failed validation.");
        }

        Log.Information("  [PaymentValidator] PASS");
        return await PassToNextAsync(order);
    }
}
