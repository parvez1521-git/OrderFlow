# AdminPortal

This is the Angular administrator portal for OrderFlow. Admin users use it to manage products, customers, inventory, and orders.

It talks to the YARP gateway, which routes requests to the microservices:

```text
Gateway  http://localhost:6200/api
```

## What This Project Does

- Lets admins create, update, and delete products.
- Lets admins manage customers.
- Lets admins set inventory quantity for products.
- Lets admins view and manage orders.

## Folder Structure

```text
AdminPortal/
  public/
  src/
    app/
      features/
        auth/
        customers/
        inventory/
        orders/
        products/
      shared/
      app.config.ts
      app.css
      app.html
      app.routes.ts
      app.ts
    main.ts
    styles.css
  angular.json
  package.json
  tsconfig.json
```

## Important Folders

`src/app/`

Contains the Angular application code.

`src/app/features/`

Contains feature-specific code. This mirrors enterprise Angular structure where each business capability gets its own folder.

`src/app/features/auth/`

Contains login API code and the HTTP interceptor that attaches the JWT token.

`src/app/features/products/`

Contains the product API service.

`src/app/features/customers/`

Contains the customer API service.

`src/app/features/inventory/`

Contains the inventory API service.

`src/app/features/orders/`

Contains the order API service.

`src/app/shared/`

Contains shared TypeScript models used across features.

## Key Files

`src/app/app.ts`

The root Angular component class. It stores screen state and calls feature API services.

`src/app/app.html`

The root Angular template. It renders login, navigation tabs, forms, tables, and buttons.

`src/app/app.css`

Styles for the admin portal.

`src/app/app.config.ts`

Application-level dependency injection setup. It registers routing and `HttpClient` with the auth interceptor.

`src/app/features/auth/auth.interceptor.ts`

Adds this header to API calls after login:

```text
Authorization: Bearer TOKEN
```

`src/app/shared/models.ts`

Shared TypeScript types for customers, products, inventory, orders, and auth responses.

## Run The Admin Portal

Start the microservices and gateway first:

```powershell
cd C:\Training2026\OrderFlow
dotnet run --project .\Services\CustomerService\OrderFlow.CustomerService.csproj
dotnet run --project .\Services\ProductService\OrderFlow.ProductService.csproj
dotnet run --project .\Services\InventoryService\OrderFlow.InventoryService.csproj
dotnet run --project .\Services\OrderService\OrderFlow.OrderService.csproj
dotnet run --project .\Gateway\OrderFlow.Gateway.csproj
```

Use separate terminals for the services.

Then start Angular:

```powershell
cd C:\Training2026\OrderFlow\AdminPortal
npm install
npm start
```

Open:

```text
http://localhost:4200
```

## Admin Workflow

1. Create or edit products.
2. Set inventory for products.
3. View or manage customers.
4. View or manage orders.
5. Use Refresh if another portal changes data.

## Angular Concepts In This Project

Components:

The UI building blocks. `app.ts`, `app.html`, and `app.css` together define the root component.

Templates:

HTML files with Angular syntax such as `@if`, `@for`, `(click)`, and `[(ngModel)]`.

Services:

Reusable classes that call APIs. Example: `ProductApiService`.

Dependency Injection:

Angular creates services and gives them to components using `inject(...)`.

HTTP Interceptor:

A central place to modify outgoing HTTP requests. This project still contains an auth interceptor from the monolith phase, but the Phase 10 microservices do not require JWT yet.

## Enterprise Learning Notes

- Admin portals usually prioritize dense, practical screens over marketing-style UI.
- Services keep HTTP logic out of components.
- Shared models help the frontend match backend contracts.
- Feature folders make it easier to grow the app without one giant folder.
- Interceptors are useful for cross-cutting behavior such as authentication, logging, and error handling.
