using Serilog;

namespace Facade.App.Subsystems;

public class LoyaltyService
{
    // 10 points per £1 spent, rounded down.
    public async Task<int> AwardPointsAsync(string customerId, decimal amount)
    {
        await Task.Delay(10);
        var points = (int)(amount * 10);
        Log.Information("    [Loyalty] Awarded {Points} points to {CustomerId} (£{Amount:F2} × 10)",
            points, customerId, amount);
        return points;
    }
}
