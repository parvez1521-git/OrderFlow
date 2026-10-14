# CustomerPortal

This is the React customer-facing portal for OrderFlow. Customers use it to create a customer profile, browse products, add products to a local cart, checkout through the microservices workflow, clear the cart, and view their orders.

It talks to the YARP gateway, which routes requests to the microservices:

```text
Gateway  http://localhost:6200/api
```

## What This Project Does

- Shows products from the backend.
- Creates a customer profile in CustomerService.
- Lets the customer add products to the cart.
- Lets the customer clear the cart.
- Lets the customer checkout using OrderService.
- Shows the logged-in customer's orders only.

## Folder Structure

```text
CustomerPortal/
  public/
  src/
    features/
      customers/
      orders/
      products/
    shared/
    App.tsx
    App.css
    main.tsx
    index.css
  package.json
  vite.config.ts
```

## Important Folders

`src/`

Contains the actual React application source code.

`src/features/`

Contains feature-specific UI code. This is the frontend equivalent of organizing backend code by business area.

`src/features/products/`

Contains the product browsing component. It displays products and lets the user choose quantities before adding items to the cart.

`src/features/orders/`

Contains order history display logic.

`src/features/customers/`

Contains customer-related UI files. Some earlier customer creation behavior moved into auth registration, but the folder remains useful as the customer feature grows.

`src/shared/`

Contains code used by multiple features.

`src/shared/api.ts`

Small wrapper around `fetch`. It knows the API base URL and automatically adds the JWT token to requests.

`src/shared/models.ts`

Shared TypeScript types for API responses such as `Product`, `Cart`, `Order`, and `CheckoutResponse`.

## Key Files

`src/App.tsx`

The root React component. It currently holds the main screen state and coordinates login, product loading, cart actions, checkout, and order loading.

In a larger enterprise React app, this would usually be split into route-level pages, custom hooks, and feature-specific API modules.

`src/main.tsx`

The application entry point. It mounts the React app into the browser DOM.

`src/App.css`

Styles for the main customer portal layout.

`vite.config.ts`

Vite build/dev-server configuration.

`package.json`

Lists scripts and frontend dependencies.

## Run The Customer Portal

Start the microservices and gateway first:

```powershell
cd C:\Training2026\OrderFlow
dotnet run --project .\Services\CustomerService\OrderFlow.CustomerService.csproj
dotnet run --project .\Services\ProductService\OrderFlow.ProductService.csproj
dotnet run --project .\Services\InventoryService\OrderFlow.InventoryService.csproj
dotnet run --project .\Services\OrderService\OrderFlow.OrderService.csproj
dotnet run --project .\Services\PaymentService\OrderFlow.PaymentService.csproj
dotnet run --project .\Services\NotificationService\OrderFlow.NotificationService.csproj
dotnet run --project .\Gateway\OrderFlow.Gateway.csproj
```

Use separate terminals for the services.

Then start React:

```powershell
cd C:\Training2026\OrderFlow\CustomerPortal
npm install
npm run dev
```

Open:

```text
http://localhost:5173
```

## Customer Workflow

1. Create a customer profile.
2. Browse products.
3. Add products to cart.
4. Clear the cart if needed.
5. Choose mock payment mode:

```text
success
failed
timeout
```

6. Checkout.
7. View your orders.

## Enterprise Learning Notes

- Components render UI.
- State tracks screen data such as products, cart, user, and messages.
- Props pass data and callbacks from parent to child components.
- Shared API code avoids repeating `fetch` logic everywhere.
- TypeScript models make API data easier to understand and safer to use.
- The cart is local in the browser for this phase; checkout goes through the gateway to OrderService.
