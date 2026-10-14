# OrderFlow.Api

This is the ASP.NET Core backend for the OrderFlow learning project. It exposes HTTP APIs for authentication, products, customers, inventory, carts, checkout, orders, and mock payments.

The project is currently a modular monolith: everything runs inside one API application, but the code is separated into business feature folders. This is useful for learning because it feels like enterprise structure without requiring multiple deployed services yet.

## What This Project Does

- Stores data in SQL Server using Entity Framework Core.
- Uses ASP.NET Core Identity for users and roles.
- Uses JWT bearer tokens so the React and Angular portals can call protected APIs.
- Protects admin-only operations such as product, customer, and inventory management.
- Allows customers to register, log in, browse products, manage a cart, checkout, and view their own orders.
- Demonstrates mock payment outcomes: `success`, `failed`, and `timeout`.

## Folder Structure

```text
OrderFlow.Api/
  Data/
    OrderFlowDbContext.cs
  Features/
    Auth/
    Cart/
    Checkout/
    Customers/
    Inventory/
    Notifications/
    Orders/
    Payment/
    Products/
  Migrations/
  Services/
  Program.cs
  appsettings.json
```

## Important Folders

`Data/`

Contains the EF Core `OrderFlowDbContext`. This class maps C# models to SQL Server tables and configures relationships between tables.

`Features/`

Contains business areas. In enterprise applications, this is often called feature-based or vertical-slice organization. Instead of putting all controllers in one folder and all services in another, related files stay together.

Example:

```text
Features/Products/
  Product.cs
  ProductDtos.cs
  ProductService.cs
  ProductsController.cs
```

This makes it easier to understand and later extract a feature into a separate microservice.

`Features/Auth/`

Contains login, registration, JWT token creation, role seeding, and the custom Identity user.

`Features/Cart/`

Contains shopping cart tables, request/response models, cart service, and cart API.

`Features/Checkout/`

Coordinates the checkout workflow:

```text
Cart -> Validate -> Inventory -> Order -> Payment -> Confirm/Cancel -> Clear/Release
```

`Features/Inventory/`

Tracks available product quantity. Checkout reduces inventory after reservation. Failed payment releases inventory.

`Features/Payment/`

Contains the mock payment gateway and payment records. This is where payment idempotency is demonstrated using an `IdempotencyKey`.

`Features/Notifications/`

Currently a placeholder module boundary. It exists so you can see where notification logic would live before it becomes a separate service later.

`Migrations/`

Contains EF Core database migrations. These files describe how SQL Server schema changes over time.

`Services/`

Contains shared support classes such as reusable business exceptions.

## Key Files

`Program.cs`

The application startup file. It registers services, EF Core, Identity, JWT authentication, CORS, Swagger, and controllers.

`appsettings.json`

Contains local configuration such as the SQL Server connection string and JWT settings.

`OrderFlowDbContext.cs`

The database context. It declares tables and relationships.

## Run The API

From the solution folder:

```powershell
cd C:\Training2026\OrderFlow
dotnet run --project .\OrderFlow.Api
```

Swagger:

```text
http://localhost:5078/swagger
```

The API automatically applies EF Core migrations on startup and seeds local learning users.

## Seed Users

```text
admin@oms.local / ChangeMe123!
ordermanager@oms.local / ChangeMe123!
customer@oms.local / ChangeMe123!
```

These are development-only learning users.

## Common Endpoints

```text
POST   /api/auth/login
POST   /api/auth/register-customer
GET    /api/auth/me

GET    /api/products
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}

GET    /api/cart
POST   /api/cart/items
PUT    /api/cart/items
DELETE /api/cart/items/{productId}
DELETE /api/cart

GET    /api/inventory
PUT    /api/inventory

POST   /api/checkout
GET    /api/orders
```

## How To Test Quickly

1. Run the API.
2. Open Swagger.
3. Login using `POST /api/auth/login`.
4. Copy the returned token.
5. Click `Authorize` in Swagger and enter:

```text
Bearer YOUR_TOKEN_HERE
```

6. Call protected endpoints such as `/api/products` or `/api/orders`.

## Enterprise Learning Notes

- Controllers handle HTTP concerns.
- Services contain business logic.
- DTOs define what the API accepts and returns.
- Entities define database shape.
- `DbContext` represents a unit of work against SQL Server.
- Feature folders keep related files together.
- JWT authentication proves who the caller is.
- Roles decide what the caller is allowed to do.
- Checkout is an orchestration workflow because it coordinates multiple modules.
