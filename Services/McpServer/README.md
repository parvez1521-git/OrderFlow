# OrderFlow MCP Server (Model Context Protocol)

The **OrderFlow MCP Server** exposes OrderFlow's microservices operations (order management, product catalog, inventory status, store policies RAG, and cancellation workflows) via the **Model Context Protocol (MCP)**. This enables AI assistants like **Claude Desktop**, **Cursor IDE**, and **Antigravity** to securely discover, inspect, and invoke OrderFlow tools.

---

## 🛠 Features

- **6 Validated MCP Tools:**
  1. `orderflow_get_order(order_id)`: Fetches full details, totals, and line items.
  2. `orderflow_list_customer_orders(customer_id, limit)`: Recent orders for a customer.
  3. `orderflow_check_inventory(product_id)`: Live stock levels (`IN_STOCK`, `LOW_STOCK`, `OUT_OF_STOCK`).
  4. `orderflow_list_products(search_term, limit)`: Product catalog search and prices.
  5. `orderflow_search_policies(query, top_k)`: Semantic RAG retrieval from store policies.
  6. `orderflow_cancel_order(order_id, reason, api_key)`: Safe order cancellation with pre-flight state validation.
  7. `orderflow_place_order(product_id, quantity, customer_id, customer_email)`: Executes checkout Saga across microservices.
- **MCP Resource:**
  - `orderflow://catalog/summary`: Live catalog snapshot for context injection.
- **MCP Prompt Template:**
  - `order_support_assistant`: Structured guidance for customer support workflows.
- **Dual Transport:**
  - **stdio** (default for local AI clients like Claude Desktop & Cursor).
  - **SSE** (Server-Sent Events over HTTP for remote or web agents).
- **Validation & Guardrails:**
  - Strict Pydantic schema validation (e.g. positive IDs, minimum reason length).
  - Pre-flight status checks prevent invalid cancellations (e.g., delivered or already cancelled orders).
  - Optional `ORDERFLOW_MCP_KEY` API key authorization check.

---

## 🚀 Quick Start

### 1. Install Dependencies
```bash
cd Services/McpServer
pip install -r requirements.txt
```

### 2. Verify with the Test Suite
```bash
python test_client.py
```

### 3. Run MCP Server

#### Option A: stdio Transport (For Claude Desktop / Cursor)
```bash
python server.py
```

#### Option B: SSE Transport (For Network AI Clients)
```bash
python server.py --sse --port 8001
```

---

## 🤖 AI Client Configuration

### Claude Desktop Integration
Add the following snippet to your `claude_desktop_config.json`:
- **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "orderflow": {
      "command": "python",
      "args": [
        "c:\\Training2026\\OrderFlow\\Services\\McpServer\\server.py"
      ],
      "env": {
        "ORDERFLOW_GATEWAY_URL": "http://localhost:6200/api",
        "ORDERFLOW_KNOWLEDGE_URL": "http://localhost:8000/api"
      }
    }
  }
}
```

### Cursor IDE Integration
Add to your project's `.cursor/mcp.json` or Global Cursor Settings:
```json
{
  "mcpServers": {
    "orderflow": {
      "command": "python",
      "args": [
        "c:\\Training2026\\OrderFlow\\Services\\McpServer\\server.py"
      ]
    }
  }
}
```

---

## 📋 Tool Reference

| Tool Name | Parameters | Description |
|---|---|---|
| `orderflow_get_order` | `order_id` (int, > 0) | Retrieve complete details of an order. |
| `orderflow_list_customer_orders` | `customer_id` (int, > 0), `limit` (int, 1-50) | Retrieve orders for a given customer. |
| `orderflow_check_inventory` | `product_id` (int, > 0) | Check available inventory quantity and stock status. |
| `orderflow_list_products` | `search_term` (str), `limit` (int, 1-50) | Search or list items in the product catalog. |
| `orderflow_search_policies` | `query` (str, >= 2 chars), `top_k` (int, 1-10) | Semantic search of store policies using RAG. |
| `orderflow_cancel_order` | `order_id` (int, > 0), `reason` (str, >= 5 chars), `api_key` (optional str) | Safely cancel an order if status allows. |
