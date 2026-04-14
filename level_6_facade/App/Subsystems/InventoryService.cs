using Facade.App.Models;
using Serilog;

namespace Facade.App.Subsystems;

// Each subsystem is independently complex — its own retries, error handling,
// external API calls. The Facade coordinates them without embedding their logic.
public class InventoryService
{
    public async Task ReserveAsync(List<CartItem> items)
    {
        await Task.Delay(20); // simulate warehouse API call
        foreach (var item in items)
            Log.Information("    [Inventory] Reserved {Qty}x '{Name}' (SKU: {Sku})",
                item.Quantity, item.Name, item.Sku);
    }

    public async Task ReleaseAsync(List<CartItem> items)
    {
        await Task.Delay(10);
        foreach (var item in items)
            Log.Information("    [Inventory] Released reservation for {Qty}x '{Name}'",
                item.Quantity, item.Name);
    }
}
