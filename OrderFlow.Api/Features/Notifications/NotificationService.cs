/* Phase 8 placeholder module boundary.
 * Notifications stay in-process for now; Phase 9+ can extract this module.
 */
namespace OrderFlow.Api.Features.Notifications;

public interface INotificationService
{
    Task NotifyOrderConfirmedAsync(int orderId, CancellationToken cancellationToken);
}

public sealed class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public Task NotifyOrderConfirmedAsync(int orderId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order {OrderId} confirmed. Notification module boundary reached.", orderId);
        return Task.CompletedTask;
    }
}
