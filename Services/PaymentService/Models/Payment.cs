/* Payment record owned by the Payment microservice. */
namespace OrderFlow.PaymentService.Models;

public sealed class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public enum PaymentStatus { Succeeded, Failed, Timeout }
