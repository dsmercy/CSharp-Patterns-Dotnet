namespace AbstractFactory.App.Interfaces;

public record RefundResult(bool Success, string RefundId, string Message);

// Abstract Product B — issue a full or partial refund against a prior transaction.
public interface IRefundProcessor
{
    RefundResult Refund(string transactionId, decimal amount, string reason);
}
