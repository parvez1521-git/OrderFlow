/* Notification record owned by the Notification microservice. */
namespace OrderFlow.NotificationService.Models;

public sealed class Notification
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SourceEventId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
