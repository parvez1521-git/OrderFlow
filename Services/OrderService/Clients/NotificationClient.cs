/* Typed HTTP client for synchronous notification calls to NotificationService. */
using System.Net.Http.Json;

namespace OrderFlow.OrderService.Clients;

public interface INotificationClient
{
    Task NotifyOrderCreatedAsync(int orderId, string recipient, string message, CancellationToken cancellationToken);
}

public sealed class NotificationClient(HttpClient http) : INotificationClient
{
    public async Task NotifyOrderCreatedAsync(int orderId, string recipient, string message, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/notifications", new NotificationRequest(orderId, recipient, message), ct);
        if (!response.IsSuccessStatusCode) throw new DownstreamServiceException("NotificationService", await response.Content.ReadAsStringAsync(ct));
    }
}

public sealed record NotificationRequest(int OrderId, string Recipient, string Message);
