namespace AbstractFactory.App.Interfaces;

public record ChargeResult(bool Success, string TransactionId, string Message);

// Abstract Product A — charge the customer's payment method.
public interface IPaymentProcessor
{
    ChargeResult Charge(string orderId, decimal amount, string currency);
}
