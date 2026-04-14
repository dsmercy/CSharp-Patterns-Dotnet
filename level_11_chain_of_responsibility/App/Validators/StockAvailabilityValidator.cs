using Chain.App.Models;
using Serilog;

namespace Chain.App.Validators;

// PATTERN CONCEPT: Concrete Handler — cheap check, runs first.
// Cheap checks belong at the front of the chain — no point running a
// fraud-detection API call if the item is simply out of stock.
public class StockAvailabilityValidator : OrderValidator
{
    public override async Task<ValidationResult> ValidateAsync(Order order)
    {
        await Task.Delay(10); // simulate stock DB lookup

        if (order.Amount > 10_000)
        {
            // PATTERN CONCEPT: handler rejects — chain stops here.
            Log.Warning("  [StockValidator]   FAIL — amount £{Amount:F2} exceeds stock limit",
                order.Amount);
            return ValidationResult.Fail(
                "STOCK_UNAVAILABLE", "Quantity exceeds available stock.");
        }

        Log.Information("  [StockValidator]   PASS");
        // PATTERN CONCEPT: pass to next handler in the chain.
        return await PassToNextAsync(order);
    }
}
