/* Payment persistence model for Phase 7 mock payments and idempotency demos. */
namespace OrderFlow.Api.Models;

public sealed class PaymentRecord
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Order Order { get; set; } = null!;
}

public enum PaymentStatus { Succeeded, Failed, Timeout }
