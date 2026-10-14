# Phase 9-17 Microservices

This folder contains the first lightweight extraction of the OrderFlow modular monolith into independent services.

The goal of Phase 9 is not to make production-perfect microservices. The goal is to make service boundaries visible:

- Each service has its own API project.
- Each service has its own EF Core `DbContext`.
- Each service has its own SQL Server LocalDB database.
- Services reference another service's data by ID only.
- No service directly joins to another service's database.

Phase 10 adds synchronous HTTP communication. `OrderService` now calls:

```text
ProductService       to read product name and price
InventoryService     to reserve/release stock
PaymentService       to process mock payment
NotificationService  to create a notification record
```

Phases 11-17 add RabbitMQ messaging, Saga state, resilience, Redis caching, YARP routing, OpenTelemetry tracing, and Aspire orchestration.

## Services

```text
Services/
  CustomerService/
    Controllers/
    Data/
    Dtos/
    Models/
  ProductService/
    Controllers/
    Data/
    Dtos/
    Models/
  InventoryService/
    Controllers/
    Data/
    Dtos/
    Models/
  OrderService/
    Controllers/
    Data/
    Dtos/
    Models/
  PaymentService/
    Controllers/
    Data/
    Dtos/
    Models/
  NotificationService/
    Controllers/
    Data/
    Dtos/
    Models/
```

## Enterprise Folder Pattern

Each service follows the same familiar ASP.NET Core structure:

```text
Controllers/
  HTTP endpoints. Controllers receive requests and return responses.

Data/
  EF Core DbContext. This is the service's private database boundary.

Dtos/
  Request/response contracts. These are the shapes sent over HTTP.

Models/
  Database/domain entities owned by the service.

Program.cs
  Startup/composition file. Registers controllers, Swagger, and EF Core.
```

Example:

```text
ProductService/
  Controllers/ProductsController.cs
  Data/ProductDbContext.cs
  Dtos/ProductDtos.cs
  Models/Product.cs
  Program.cs
```

## Ports

```text
CustomerService      http://localhost:6101
ProductService       http://localhost:6102
InventoryService     http://localhost:6103
OrderService         http://localhost:6104
PaymentService       http://localhost:6105
NotificationService  http://localhost:6106
```

Each service exposes Swagger at `/swagger`.

## Database Ownership

```text
CustomerService      OrderFlow.CustomerService
ProductService       OrderFlow.ProductService
InventoryService     OrderFlow.InventoryService
OrderService         OrderFlow.OrderService
PaymentService       OrderFlow.PaymentService
NotificationService  OrderFlow.NotificationService
```

This is the main learning point of Phase 9. In the modular monolith, one database stores everything. In microservices, each service owns its own data.

For example, `OrderService` stores:

```text
CustomerId
ProductId
```

but it does not have foreign keys to the customer or product database. Those IDs are external references.

## Run One Service

From the solution folder:

```powershell
cd C:\Training2026\OrderFlow
dotnet run --project .\Services\ProductService\OrderFlow.ProductService.csproj
```

Then open:

```text
http://localhost:6102/swagger
```

## Run All Services

Use separate terminals:

```powershell
dotnet run --project .\Services\CustomerService\OrderFlow.CustomerService.csproj
dotnet run --project .\Services\ProductService\OrderFlow.ProductService.csproj
dotnet run --project .\Services\InventoryService\OrderFlow.InventoryService.csproj
dotnet run --project .\Services\OrderService\OrderFlow.OrderService.csproj
dotnet run --project .\Services\PaymentService\OrderFlow.PaymentService.csproj
dotnet run --project .\Services\NotificationService\OrderFlow.NotificationService.csproj
```

The React and Angular portals now call the YARP gateway at `http://localhost:6200`, so run the gateway too.

## Phase 10 Checkout Flow

The customer portal sends checkout to:

```text
POST http://localhost:6104/api/checkout
```

Then `OrderService` performs synchronous HTTP calls:

```text
OrderService
  -> ProductService
  -> InventoryService
  -> PaymentService
  -> NotificationService
```

If InventoryService, ProductService, PaymentService, or NotificationService is stopped, checkout returns an error. This demonstrates service coupling and failure propagation in synchronous communication.

## What Each Service Owns

`CustomerService`

Owns customer profile data such as name and email.

`ProductService`

Owns product catalog data such as product name and price.

`InventoryService`

Owns stock quantity. It can reserve and release stock.

`OrderService`

Owns order records and order lines. It stores customer and product IDs as references.

`PaymentService`

Owns payment records and mock payment processing. It demonstrates idempotency using an idempotency key.

`NotificationService`

Owns notification records. It stores queued/sent notification messages.

## Phase Boundary

This phase creates independent services and independent databases.

Phase 10 has added synchronous HTTP communication between services using `HttpClientFactory`, typed clients, timeouts, and error handling.

Docker, Kubernetes, CI/CD, and cloud work are later phases and are intentionally not implemented here.
