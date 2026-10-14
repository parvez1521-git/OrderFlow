/* Mock payment gateway for Phase 7.
 * It intentionally simulates success, failure, and timeout without external APIs.
 */
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Features.Payment;

public interface IPaymentGateway
{
    Task<PaymentResponse> ProcessAsync(int orderId, decimal amount, string mode, string idempotencyKey, CancellationToken cancellationToken);
}

public sealed class MockPaymentGateway(OrderFlowDbContext db) : IPaymentGateway
{
    public async Task<PaymentResponse> ProcessAsync(int orderId, decimal amount, string mode, string idempotencyKey, CancellationToken ct)
    {
        var existing = await db.PaymentRecords.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);
        if (existing is not null) return ToResponse(existing);

        var normalizedMode = mode.Trim().ToLowerInvariant();
        var status = normalizedMode switch
        {
            "failed" or "fail" => PaymentStatus.Failed,
            "timeout" => PaymentStatus.Timeout,
            _ => PaymentStatus.Succeeded
        };
        if (status == PaymentStatus.Timeout) await Task.Delay(500, ct);

        var payment = new PaymentRecord
        {
            OrderId = orderId,
            Amount = amount,
            IdempotencyKey = idempotencyKey.Trim(),
            Status = status,
            FailureReason = status == PaymentStatus.Succeeded ? null : $"Mock payment {status.ToString().ToLowerInvariant()}",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.PaymentRecords.Add(payment);
        await db.SaveChangesAsync(ct);
        return ToResponse(payment);
    }

    private static PaymentResponse ToResponse(PaymentRecord payment) => new(payment.Id, payment.OrderId, payment.Amount, payment.Status, payment.FailureReason, payment.IdempotencyKey);
}
