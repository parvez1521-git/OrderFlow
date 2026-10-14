# OrderFlow - Phases 3-19

A comprehensive .NET 10 learning solution featuring a React customer portal, an Angular admin portal, a YARP API gateway, and extracted microservices for customer, product, inventory, order, payment, and notification responsibilities.

The original `OrderFlow.Api` project serves as the authentication provider (JWT) and monolith baseline. The Phase 17+ runtime architecture is **gateway-first**: both frontend portals send requests through `Gateway` (`http://localhost:6200`), which proxies and routes requests to the microservices under `Services/`. The Angular admin portal authenticates directly with `OrderFlow.Api` (`http://localhost:5078`).

---

## Repository Structure

```text
OrderFlow/
├── OrderFlow.Api/              # Monolith & Auth service (JWT authentication, seed users) - :5078
├── Gateway/                    # YARP Reverse Proxy & API Gateway - :6200
├── Services/                   # Extracted standalone microservices
│   ├── CustomerService/        # Customer profiles and management - :6101
│   ├── ProductService/         # Product catalog and Redis cache - :6102
│   ├── InventoryService/       # Stock tracking and reservation/release - :6103
│   ├── OrderService/           # Orders, Saga orchestrator, and RabbitMQ publisher - :6104
│   ├── PaymentService/         # Mock payment processing - :6105
│   ├── NotificationService/    # Notifications and RabbitMQ consumer - :6106
│   ├── AiService/              # Ollama Local LLM & AI Order Assistant - :6107
│   ├── KnowledgeService/       # Python RAG Policy & Ingestion service - :8000
│   └── McpServer/              # Model Context Protocol (MCP) server - :8001 / stdio
├── CustomerPortal/             # Customer React + Vite web application (AI Assistant) - :5173
├── AdminPortal/                # Administrator Angular web application (RAG Policies) - :4200
├── OrderFlow.AppHost/          # .NET Aspire distributed application orchestrator
├── docker/                     # Dockerfile definitions for .NET services
├── docker-compose.yml          # Full local Docker Compose environment (Phase 18)
├── k8s/                        # Kubernetes deployment & service manifests (Phase 19)
├── scripts/                    # Helper scripts for Docker Compose and Kubernetes
├── docs/                       # Architecture, messaging, and concept guides
├── run-orderflow.bat           # One-click Windows startup script for all services & portals
└── OrderFlow.slnx              # Visual Studio solution file
```

---

## Deliberate Scope

* **Included:** Identity & JWT authentication, role authorization, protected APIs, shopping cart, inventory reservation/release, order checkout saga with compensation, mock payments, EF Core SQL Server persistence, Swagger/OpenAPI, React customer journey, Angular admin management, standalone microservices, synchronous HTTP service-to-service calls, RabbitMQ asynchronous messaging, Saga state pattern, Polly resilience policies, Redis distributed caching, YARP Gateway, OpenTelemetry tracing, .NET Aspire AppHost, local Docker Compose, and local Kubernetes manifests.
* **Excluded:** Cloud deployment, online container registry publishing, production Kubernetes ingress/SSL certificates, and CI/CD deployment pipelines.

---

## Seed Logins

When `OrderFlow.Api` starts against an updated database, the following local development accounts are seeded:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@oms.local` | `ChangeMe123!` |
| **Order Manager** | `ordermanager@oms.local` | `ChangeMe123!` |
| **Customer** | `customer@oms.local` | `ChangeMe123!` |

---

## Ways to Run OrderFlow

### Option 1: One-Click Startup Script (Local Windows)

The easiest way to start all 8 backend/gateway services and both frontend portals locally:

```powershell
cd C:\Training2026\OrderFlow
.\run-orderflow.bat
```

This launches dedicated console windows for:
1. `OrderFlow.Api` (Auth) on `:5078`
2. `CustomerService` on `:6101`
3. `ProductService` on `:6102`
4. `InventoryService` on `:6103`
5. `OrderService` on `:6104`
6. `PaymentService` on `:6105`
7. `NotificationService` on `:6106`
8. `Gateway` on `:6200`
9. React Customer Portal on `:5173`
10. Angular Admin Portal on `:4200`

---

### Option 2: Docker Compose (Phase 18)

Run the complete containerized stack (including SQL Server, Redis, and RabbitMQ):

```powershell
cd C:\Training2026\OrderFlow
.\scripts\docker-up.ps1
```

Or run detached via standard Docker Compose:
```powershell
docker compose up -d
```

To stop all containers:
```powershell
.\scripts\docker-down.ps1
```

*See [docs/architecture/phase-18-docker.md](docs/architecture/phase-18-docker.md) for details.*

---

### Option 3: .NET Aspire Orchestrator (Phase 17)

If Docker Desktop is running, .NET Aspire can orchestrate the microservices, gateway, Redis, and RabbitMQ automatically:

```powershell
cd C:\Training2026\OrderFlow
dotnet run --project .\OrderFlow.AppHost\OrderFlow.AppHost.csproj
```

Open the printed Aspire dashboard URL to inspect live traces, logs, and service health.

---

### Option 4: Manual Local Setup (Step-by-Step)

#### 1. Prerequisites & Database Setup
Requires .NET 10 SDK and SQL Server LocalDB.

```powershell
cd C:\Training2026\OrderFlow
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\OrderFlow.Api --startup-project .\OrderFlow.Api --configuration Release
```

*(If LocalDB fails to start: `sqllocaldb start mssqllocaldb`)*

#### 2. Start Auth API & Gateway
```powershell
# Terminal 1: Auth API
dotnet run --project .\OrderFlow.Api

# Terminal 2: Gateway
dotnet run --project .\Gateway\OrderFlow.Gateway.csproj
```

#### 3. Start Extracted Microservices
Run each service in `Services/` (or see [Services/README.md](Services/README.md)):
```powershell
dotnet run --project .\Services\CustomerService\OrderFlow.CustomerService.csproj
dotnet run --project .\Services\ProductService\OrderFlow.ProductService.csproj
dotnet run --project .\Services\InventoryService\OrderFlow.InventoryService.csproj
dotnet run --project .\Services\OrderService\OrderFlow.OrderService.csproj
dotnet run --project .\Services\PaymentService\OrderFlow.PaymentService.csproj
dotnet run --project .\Services\NotificationService\OrderFlow.NotificationService.csproj
```

#### 4. Start Frontends
```powershell
# Customer Portal (React / Vite)
cd C:\Training2026\OrderFlow\CustomerPortal
npm install
npm run dev

# Admin Portal (Angular)
cd C:\Training2026\OrderFlow\AdminPortal
npm install
npm start
```

---

### Option 5: Local Kubernetes (Phase 19)

Deploy pods, deployments, services, ConfigMaps, and secrets locally using KinD or Minikube:

```powershell
cd C:\Training2026\OrderFlow
.\scripts\k8s-build-images.ps1
.\scripts\k8s-apply.ps1
```

To clean up:
```powershell
.\scripts\k8s-delete.ps1
```

*See [docs/architecture/phase-19-kubernetes.md](docs/architecture/phase-19-kubernetes.md) for details.*

---

## Active URLs and Ports

| Component | Technology | URL | Description |
| :--- | :--- | :--- | :--- |
| **Customer Portal** | React 19, Vite | `http://localhost:5173` | Customer store, cart, and checkout |
| **Admin Portal** | Angular 21 | `http://localhost:4200` | Management portal for products, orders, stock |
| **YARP Gateway** | .NET 10, YARP | `http://localhost:6200` | Unified API routing; `/health` endpoint |
| **Auth API** | .NET 10, EF Core | `http://localhost:5078` | JWT login, registration, and monolith base |
| **Customer Service** | .NET 10, Web API | `http://localhost:6101` | Customer profiles and CRUD |
| **Product Service** | .NET 10, Redis | `http://localhost:6102` | Product catalog with distributed caching |
| **Inventory Service** | .NET 10, Web API | `http://localhost:6103` | Stock reservation & release |
| **Order Service** | .NET 10, EF Core | `http://localhost:6104` | Order placement, Saga state, RabbitMQ publish |
| **Payment Service** | .NET 10, Web API | `http://localhost:6105` | Mock payment gateway (`success`, `fail`, `timeout`) |
| **Notification Service**| .NET 10, RabbitMQ | `http://localhost:6106` | Notification log and event consumer |
| **AiService** | .NET 10, Ollama | `http://localhost:6107` | Local LLM (`llama3.2`) tool-calling assistant |
| **KnowledgeService** | Python 3, FastAPI | `http://localhost:8000` | RAG policy ingestion & vector retrieval |
| **McpServer** | Python 3, MCP SDK | `http://localhost:8001` | Model Context Protocol server (stdio / SSE) |
| **RabbitMQ Management** | RabbitMQ 3 | `http://localhost:15672` | Broker dashboard (`guest` / `guest`) |
| **SQL Server** | SQL Server 2022 | `localhost:14333` | Database container (Docker mode) |
| **Redis** | Redis 7 | `localhost:6379` | Distributed cache (Docker/Aspire mode) |

---

## Gateway Routing & API Endpoints

The YARP Gateway proxies all requests through `http://localhost:6200/api/*` using development Basic authentication (`orderflow-admin` / `orderflow-dev-password`), automatically included by the portals.

### Microservice Endpoints (via Gateway)
* **Customers:** `GET /api/customers`, `POST /api/customers`, `GET /api/customers/{id}`
* **Products:** `GET /api/products`, `POST /api/products`, `GET /api/products/{id}`
* **Inventory:** `GET /api/inventory`, `POST /api/inventory/reserve`, `POST /api/inventory/release`
* **Orders:** `GET /api/orders`, `POST /api/orders`, `GET /api/orders/{id}`
* **Checkout (Saga):** `POST /api/checkout` (orchestrated checkout across inventory, payment, and notifications)
* **Saga Audit:** `GET /api/checkout/sagas` (inspect saga execution history and compensation states)
* **Payments:** `GET /api/payments`, `POST /api/payments/process`
* **Notifications:** `GET /api/notifications`, `POST /api/notifications`

### Auth & Monolith Endpoints (`http://localhost:5078`)
* `POST /api/auth/login`
* `POST /api/auth/register-customer`
* `GET /api/auth/me`

---

## Architectural Guides & Concept Documentation

Deep-dive documentation for specific concepts can be found in the `docs/` folder:

* **RabbitMQ & Messaging (Phase 11):** [docs/messaging/phase-11-rabbitmq.md](docs/messaging/phase-11-rabbitmq.md)
* **Saga Pattern & Compensation (Phase 12):** [docs/saga/phase-12-saga.md](docs/saga/phase-12-saga.md)
* **Resilience & Polly Policies (Phase 13):** [docs/architecture/phase-13-resilience.md](docs/architecture/phase-13-resilience.md)
* **Redis Caching (Phase 14):** [docs/architecture/phase-14-redis.md](docs/architecture/phase-14-redis.md)
* **YARP Reverse Proxy (Phase 15):** [docs/architecture/phase-15-yarp.md](docs/architecture/phase-15-yarp.md)
* **OpenTelemetry Tracing (Phase 16):** [docs/architecture/phase-16-opentelemetry.md](docs/architecture/phase-16-opentelemetry.md)
* **.NET Aspire Orchestration (Phase 17):** [docs/architecture/phase-17-aspire.md](docs/architecture/phase-17-aspire.md)
* **Docker Architecture (Phase 18):** [docs/architecture/phase-18-docker.md](docs/architecture/phase-18-docker.md)
* **Kubernetes Architecture (Phase 19):** [docs/architecture/phase-19-kubernetes.md](docs/architecture/phase-19-kubernetes.md)
* **Concepts Guide Document:** [OrderFlow_Implemented_Concepts_Guide.docx](OrderFlow_Implemented_Concepts_Guide.docx)

---

## AI & MCP Capabilities

1. **AI Order Assistant:**
   - Interactive floating assistant embedded in the React Customer Portal (`:5173`).
   - Answers questions about order history, order status, product availability, and policies.
2. **Local LLM Integration (Ollama):**
   - Runs local `llama3.2` model via Ollama on `http://localhost:11434`.
   - `.NET 10` `AiService` (`:6107`) uses `Microsoft.Extensions.AI.IChatClient` with automatic function/tool calling against the microservices Gateway.
3. **Python + RAG Service:**
   - FastAPI service (`:8000`) for store policy ingestion and TF-IDF cosine vector retrieval.
   - Admin Portal (`:4200`) provides a dedicated "Policies (RAG)" management interface with full CRUD and editing capabilities.
4. **Model Context Protocol (MCP) Server:**
   - Exposes 6 validated, authorized OrderFlow operations via standard MCP protocol (`Services/McpServer/`).
   - Supports stdio transport for Claude Desktop / Cursor / Antigravity and SSE transport (`:8001`) for web agents.
   - Documentation & sample configs: [Services/McpServer/README.md](Services/McpServer/README.md).

