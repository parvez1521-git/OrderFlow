/* Payment request/response contracts. */
using System.ComponentModel.DataAnnotations;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Features.Payment;

public sealed record MockPaymentRequest([Required] string Mode, [Required] string IdempotencyKey);
public sealed record PaymentResponse(int Id, int OrderId, decimal Amount, PaymentStatus Status, string? FailureReason, string IdempotencyKey);
