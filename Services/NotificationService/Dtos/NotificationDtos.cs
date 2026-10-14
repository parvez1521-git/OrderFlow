/* Notification request contracts for this microservice. */
namespace OrderFlow.NotificationService.Dtos;

public sealed record NotificationRequest(int OrderId, string Recipient, string Message);
