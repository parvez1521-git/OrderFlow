/* Payment request contracts for this microservice. */
namespace OrderFlow.PaymentService.Dtos;

public sealed record PaymentRequest(int OrderId, decimal Amount, string Mode, string IdempotencyKey);
