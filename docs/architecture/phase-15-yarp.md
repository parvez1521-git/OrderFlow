# Phase 15 - YARP API Gateway

YARP provides one front door for the microservices.

```text
React / Angular
      |
      v
YARP Gateway :6200
      |
      +-- CustomerService
      +-- ProductService
      +-- InventoryService
      +-- OrderService
      +-- PaymentService
      +-- NotificationService
```

## Implemented Pieces

- Gateway project: `Gateway/OrderFlow.Gateway.csproj`
- Routes configured in `Gateway/appsettings.json`
- Gateway port: `http://localhost:6200`
- Rate limiting
- Health endpoint
- OpenTelemetry tracing

## Learning Point

The gateway should route, secure, and observe traffic. It should not contain business logic.
