# Phase 18 — Local Docker for OrderFlow

This phase containerizes the OrderFlow application for local learning only. Nothing is pushed to Docker Hub, Azure, AWS, or any other online server.

## What Docker gives us here

Docker lets each part of the system run in its own isolated container:

- SQL Server container stores local development databases.
- Redis container supports product caching.
- RabbitMQ container supports event messaging.
- Each .NET service runs in its own backend container.
- React and Angular are built into static files and served by Nginx containers.

## Important local URLs

After running Docker Compose:

- Customer Portal: `http://localhost:5173`
- Admin Portal: `http://localhost:4200`
- Auth API: `http://localhost:5078`
- Gateway: `http://localhost:6200`
- RabbitMQ dashboard: `http://localhost:15672`
- SQL Server host port: `localhost,14333`

RabbitMQ default login is:

- Username: `guest`
- Password: `guest`

SQL Server local container login is:

- Server: `localhost,14333`
- User: `sa`
- Password: `OrderFlow_Local_12345!`

## How to run locally

From PowerShell:

```powershell
cd C:\Training2026\OrderFlow
.\scripts\docker-up.ps1
```

This runs:

```powershell
docker compose up --build
```

To stop containers but keep the database data:

```powershell
.\scripts\docker-down.ps1
```

To stop containers and remove the local SQL Server volume:

```powershell
.\scripts\docker-down.ps1 -RemoveData
```

## Files added in this phase

- `.dockerignore` keeps Docker builds fast by excluding `bin`, `obj`, `node_modules`, and generated artifacts.
- `docker/dotnet-service.Dockerfile` builds any .NET backend service using build arguments.
- `CustomerPortal/Dockerfile` builds and serves the React portal.
- `AdminPortal/Dockerfile` builds and serves the Angular portal.
- `docker-compose.yml` defines the complete local container environment.
- `scripts/docker-up.ps1` starts the full Docker stack.
- `scripts/docker-down.ps1` stops the Docker stack.

## Beginner mental model

When running without Docker, your services talk to `localhost` ports directly.

When running inside Docker, containers talk to each other using service names:

- `product-service`
- `inventory-service`
- `order-service`
- `sqlserver`
- `redis`
- `rabbitmq`

The browser still uses `localhost` because the browser is running on your laptop, not inside Docker.
