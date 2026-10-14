/* Typed HTTP client for synchronous payment calls to PaymentService. */
using System.Net.Http.Json;

namespace OrderFlow.OrderService.Clients;

public interface IPaymentClient
{
    Task<PaymentResult> ProcessAsync(int orderId, decimal amount, string mode, string idempotencyKey, CancellationToken cancellationToken);
}

public sealed class PaymentClient(HttpClient http) : IPaymentClient
{
    public async Task<PaymentResult> ProcessAsync(int orderId, decimal amount, string mode, string idempotencyKey, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/payments/process", new PaymentRequest(orderId, amount, mode, idempotencyKey), ct);
        if (!response.IsSuccessStatusCode) throw new DownstreamServiceException("PaymentService", await response.Content.ReadAsStringAsync(ct));
        return (await response.Content.ReadFromJsonAsync<PaymentResult>(cancellationToken: ct))!;
    }
}

public sealed record PaymentRequest(int OrderId, decimal Amount, string Mode, string IdempotencyKey);
public sealed record PaymentResult(int Id, int OrderId, decimal Amount, string Status, string? FailureReason, DateTime CreatedAtUtc);
