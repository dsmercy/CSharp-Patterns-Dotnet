namespace Adapter.App.Modern;

public record PaymentResult(bool Success, string TransactionId, string AuthCode, string ErrorCode);
public record RefundResult(bool Success, string RefundId, string ErrorCode);

// PATTERN CONCEPT: the Target interface.
//
// This is the contract the rest of the codebase depends on — clean, modern, async.
// CheckoutService is written against this interface only. It has no idea whether
// the underlying implementation is a legacy XML gateway, a REST API, or a mock.
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(string orderId, decimal amount, string currency);
    Task<RefundResult>  RefundAsync(string transactionId, decimal amount, string reason);
}
