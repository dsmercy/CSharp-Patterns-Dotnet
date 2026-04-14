using Chain.App.Models;
using Serilog;

namespace Chain.App.Validators;

// PATTERN CONCEPT: Concrete Handler — expensive external API call.
// Placed after cheap checks so it only runs for orders that passed stock
// and payment checks. Order in the chain is a performance decision.
public class FraudDetectionValidator : OrderValidator
{
    public override async Task<ValidationResult> ValidateAsync(Order order)
    {
        await Task.Delay(80); // simulate fraud-detection API round-trip

        if (order.IsFraudulent)
        {
            Log.Warning("  [FraudValidator]   FAIL — order flagged as fraudulent");
            return ValidationResult.Fail(
                "FRAUD_DETECTED", "Order flagged by fraud detection service.");
        }

        Log.Information("  [FraudValidator]   PASS");
        return await PassToNextAsync(order);
    }
}
