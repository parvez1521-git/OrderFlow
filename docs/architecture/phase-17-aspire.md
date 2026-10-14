# Phase 17 - .NET Aspire

The `OrderFlow.AppHost` project introduces Aspire local orchestration.

## Implemented Pieces

- AppHost project: `OrderFlow.AppHost`
- Orchestrates:
  - Gateway
  - CustomerService
  - ProductService
  - InventoryService
  - OrderService
  - PaymentService
  - NotificationService
  - Redis
  - RabbitMQ

## Run

```powershell
cd C:\Training2026\OrderFlow
dotnet run --project .\OrderFlow.AppHost\OrderFlow.AppHost.csproj
```

## Learning Point

Aspire helps run a distributed app locally and gives one place for service discovery, configuration, health, logs, and traces.
