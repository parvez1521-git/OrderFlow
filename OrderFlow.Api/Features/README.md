# Feature-First API Organization

Each business feature owns its controller, service, and DTO files:

- `Customers/`: customer CRUD HTTP boundary and application service.
- `Products/`: product CRUD HTTP boundary and application service.
- `Orders/`: order CRUD HTTP boundary and application service.

The entity files are physically colocated with their features but retain the `OrderFlow.Api.Models` namespace for now. This preserves the names stored in the existing EF Core migration snapshot and prevents a folder-only refactor from looking like a database schema change.

`Data/OrderFlowDbContext.cs` remains shared infrastructure because it composes the complete relational model.
