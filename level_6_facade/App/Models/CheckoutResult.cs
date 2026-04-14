namespace Facade.App.Models;

public record CheckoutResult(
    bool   Success,
    string TransactionId,
    string TrackingNumber,
    int    LoyaltyPointsAwarded,
    string Message = "");
