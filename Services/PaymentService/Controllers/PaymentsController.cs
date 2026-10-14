/* Controller for mock payment endpoints. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.PaymentService.Data;
using OrderFlow.PaymentService.Dtos;
using OrderFlow.PaymentService.Models;

namespace OrderFlow.PaymentService.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(PaymentDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<Payment>> GetAll() => await db.Payments.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Payment>> GetById(int id) => await db.Payments.FindAsync(id) is { } payment ? Ok(payment) : NotFound();

    [HttpPost("process")]
    public async Task<ActionResult<Payment>> Process(PaymentRequest request)
    {
        var existing = await db.Payments.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey);
        if (existing is not null) return Ok(existing);

        var mode = request.Mode.Trim().ToLowerInvariant();
        var status = mode switch { "failed" or "fail" => PaymentStatus.Failed, "timeout" => PaymentStatus.Timeout, _ => PaymentStatus.Succeeded };
        if (status == PaymentStatus.Timeout) await Task.Delay(500);

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            IdempotencyKey = request.IdempotencyKey.Trim(),
            Status = status,
            FailureReason = status == PaymentStatus.Succeeded ? null : $"Mock payment {status.ToString().ToLowerInvariant()}",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { payment.Id }, payment);
    }
}
