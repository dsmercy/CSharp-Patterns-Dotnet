namespace Chain.App.Models;

public record ValidationResult(bool Passed, string ErrorCode, string ErrorMessage)
{
    public static ValidationResult Success() =>
        new(true, string.Empty, string.Empty);

    public static ValidationResult Fail(string code, string message) =>
        new(false, code, message);

    public override string ToString() =>
        Passed ? "PASSED" : $"FAILED [{ErrorCode}] {ErrorMessage}";
}
