# Milestone 1 Architecture

OrderFlow is intentionally a single deployable ASP.NET Core API. Controllers handle HTTP concerns, services own CRUD rules, and EF Core persists the small initial model to SQL Server. The source is organized feature-first: each Customer, Product, and Order folder contains its controller, service, and DTO contracts, which gives each business capability a clear home without introducing microservices.

The order model includes `OrderItem` because an order without line items cannot represent a meaningful product order. It stores the product price as `UnitPrice` when the order is created. There is no total calculation, stock validation, or order processing workflow in this milestone.

EF Core relationships restrict deletion of customers and products that are referenced by orders. Deleting an order deletes its own line items.
