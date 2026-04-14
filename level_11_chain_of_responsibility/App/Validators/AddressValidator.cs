using Chain.App.Models;
using Serilog;

namespace Chain.App.Validators;

// PATTERN CONCEPT: Concrete Handler — last in the chain.
// When PassToNextAsync is called here, _next is null, so ValidationResult.Success()
// is returned automatically — the chain completes successfully.
public class AddressValidator : OrderValidator
{
    private static readonly HashSet<string> _allowedRegions = ["GB", "US", "EU"];

    public override async Task<ValidationResult> ValidateAsync(Order order)
    {
        await Task.Delay(20); // simulate address verification lookup

        if (!_allowedRegions.Contains(order.ShippingRegion))
        {
            Log.Warning("  [AddressValidator] FAIL — region '{Region}' not supported",
                order.ShippingRegion);
            return ValidationResult.Fail(
                "ADDRESS_INVALID",
                $"Shipping region '{order.ShippingRegion}' is not supported.");
        }

        Log.Information("  [AddressValidator] PASS");
        return await PassToNextAsync(order); // _next is null → Success()
    }
}
