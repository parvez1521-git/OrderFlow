# Phase 16 - OpenTelemetry

OpenTelemetry is used to demonstrate distributed tracing.

## Implemented Pieces

- Gateway ASP.NET Core tracing
- Gateway outgoing HTTP tracing
- OrderService ASP.NET Core tracing
- OrderService outgoing HTTP tracing
- Console exporter for local learning
- Health endpoints for service checks

## Example Trace Path

```text
React
  -> YARP Gateway
  -> OrderService
  -> ProductService
  -> InventoryService
  -> PaymentService
  -> NotificationService
```

RabbitMQ publishing is also logged by OrderService.

## Learning Point

When a request crosses service boundaries, tracing helps answer: where did it go, how long did each step take, and where did it fail?
