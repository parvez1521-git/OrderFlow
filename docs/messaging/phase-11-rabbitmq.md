# Phase 11 - RabbitMQ

RabbitMQ introduces asynchronous communication.

In OrderFlow, `OrderService` publishes integration events after checkout:

```text
order.confirmed
payment.failed
inventory.reserved
```

`NotificationService` consumes order events from RabbitMQ.

## Implemented Pieces

- Publisher: `OrderService/Messaging/RabbitMqEventPublisher.cs`
- Event contract: `OrderService/Messaging/IntegrationEvent.cs`
- Exchange: `orderflow.events`
- Queue: `notification-service.order-events`
- Dead-letter queue: `notification-service.order-events.dlq`
- Consumer: `NotificationService/Messaging/RabbitMqNotificationConsumer.cs`
- Duplicate handling: `Notification.SourceEventId`
- Retry demo: retry count header before dead-lettering

## Learning Point

HTTP is synchronous: the caller waits.

RabbitMQ is asynchronous: the publisher sends an event and moves on. The consumer handles the event later.
