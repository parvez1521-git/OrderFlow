/* Shared event envelope for RabbitMQ messages published by OrderService. */
namespace OrderFlow.OrderService.Messaging;

public sealed record IntegrationEvent(Guid EventId, string EventType, int OrderId, DateTime OccurredAtUtc, object Payload);
