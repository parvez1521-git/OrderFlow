"""
OrderFlow Model Context Protocol (MCP) Server.
Exposes OrderFlow operations (orders, catalog, inventory, store policies, cancellations)
to MCP-compatible AI clients (Claude Desktop, Cursor, Antigravity, etc.).
"""
import sys
import argparse
from typing import Any, Dict, Optional
from pydantic import Field
from mcp.server.mcpserver import MCPServer

from tools import (
    orderflow_get_order,
    orderflow_list_customer_orders,
    orderflow_check_inventory,
    orderflow_list_products,
    orderflow_cancel_order,
    orderflow_search_policies,
    orderflow_place_order,
)
from config import get_gateway_client, get_knowledge_client

# Initialize MCP Server
mcp = MCPServer(
    name="OrderFlow",
    version="1.0.0",
    description="Model Context Protocol Server for the OrderFlow Microservices Platform"
)


# ============================================================================
# MCP Tools
# ============================================================================

@mcp.tool(
    name="orderflow_get_order",
    description="Retrieve full details for an order by its numeric ID (status, items, total, customer ID, order date)."
)
async def get_order_tool(
    order_id: int = Field(gt=0, description="The unique numeric ID of the order.")
) -> Dict[str, Any]:
    return await orderflow_get_order(order_id)


@mcp.tool(
    name="orderflow_list_customer_orders",
    description="List orders placed by a specific customer ID, sorted by most recent first."
)
async def list_customer_orders_tool(
    customer_id: int = Field(gt=0, description="The numeric customer ID whose orders to retrieve."),
    limit: int = Field(default=10, ge=1, le=50, description="Maximum number of orders to return (1-50, default 10).")
) -> Dict[str, Any]:
    return await orderflow_list_customer_orders(customer_id, limit)


@mcp.tool(
    name="orderflow_check_inventory",
    description="Check live available stock quantity and status (IN_STOCK, LOW_STOCK, OUT_OF_STOCK) for a product."
)
async def check_inventory_tool(
    product_id: int = Field(gt=0, description="The numeric product ID to check stock for.")
) -> Dict[str, Any]:
    return await orderflow_check_inventory(product_id)


@mcp.tool(
    name="orderflow_list_products",
    description="Browse or search the product catalog by name or keyword with pricing."
)
async def list_products_tool(
    search_term: str = Field(default="", description="Optional search term to filter products by name."),
    limit: int = Field(default=20, ge=1, le=50, description="Maximum number of products to return (1-50, default 20).")
) -> Dict[str, Any]:
    return await orderflow_list_products(search_term, limit)


@mcp.tool(
    name="orderflow_cancel_order",
    description="Cancel an active order with validation and safety checks. Requires a valid cancellation reason."
)
async def cancel_order_tool(
    order_id: int = Field(gt=0, description="The unique ID of the order to cancel."),
    reason: str = Field(min_length=5, description="The reason for cancelling the order (at least 5 characters)."),
    api_key: Optional[str] = Field(default="", description="Optional MCP API key for authorization if configured.")
) -> Dict[str, Any]:
    return await orderflow_cancel_order(order_id, reason, api_key)


@mcp.tool(
    name="orderflow_search_policies",
    description="Search store policies (returns, refunds, shipping, delivery, warranties, cancellation rules) using RAG."
)
async def search_policies_tool(
    query: str = Field(min_length=2, description="The customer policy question or search topic."),
    top_k: int = Field(default=3, ge=1, le=10, description="Number of policy matches to return (1-10, default 3).")
) -> Dict[str, Any]:
    return await orderflow_search_policies(query, top_k)


@mcp.tool(
    name="orderflow_place_order",
    description="Place a new order for a product. Validates inventory stock and executes the checkout Saga across microservices."
)
async def place_order_tool(
    product_id: int = Field(gt=0, description="The numeric ID of the product to purchase (e.g. 1 for TV, 2 for Microwave)."),
    quantity: int = Field(default=1, ge=1, le=10, description="Quantity to purchase (1-10, default 1)."),
    customer_id: int = Field(default=1, gt=0, description="Customer ID placing the order (default 1)."),
    customer_email: str = Field(default="customer@oms.local", description="Customer email for confirmation.")
) -> Dict[str, Any]:
    return await orderflow_place_order(product_id, quantity, customer_id, customer_email)



# ============================================================================
# MCP Resources
# ============================================================================

@mcp.resource("orderflow://catalog/summary", description="Summary of current catalog products")
async def catalog_summary_resource() -> str:
    """Provides a live text snapshot of all products in the catalog."""
    async with get_gateway_client() as client:
        try:
            resp = await client.get("products")
            products = resp.json() if resp.is_success else []
            lines = ["# OrderFlow Product Catalog", f"Total Products: {len(products)}", ""]
            for p in products:
                lines.append(f"- [#{p.get('id')}] {p.get('name')}: ${p.get('price', 0):,.2f}")
            return "\n".join(lines)
        except Exception as ex:
            return f"Error loading catalog summary: {str(ex)}"


# ============================================================================
# MCP Prompts
# ============================================================================

@mcp.prompt("order_support_assistant", description="Assists a customer with an order inquiry using OrderFlow tools")
def order_support_prompt(order_id: int) -> str:
    return f"""You are the OrderFlow Customer Support Assistant.
The customer has a query regarding Order #{order_id}.
1. First, call 'orderflow_get_order' with order_id={order_id} to view its current status and items.
2. If the user asks about returns, refunds, or warranties, use 'orderflow_search_policies' to look up our policies.
3. If they wish to cancel the order, verify if the order is eligible for cancellation and use 'orderflow_cancel_order' with a reason.
Always be polite, concise, and helpful."""


# ============================================================================
# Main Entry Point
# ============================================================================

def main():
    parser = argparse.ArgumentParser(description="OrderFlow Model Context Protocol Server")
    parser.add_argument(
        "--transport",
        choices=["stdio", "sse"],
        default="stdio",
        help="Transport type: stdio (for Claude/Cursor/Antigravity) or sse (default: stdio)"
    )
    parser.add_argument("--sse", action="store_true", help="Shorthand for --transport sse")
    parser.add_argument("--host", default="127.0.0.1", help="Host address for SSE transport (default: 127.0.0.1)")
    parser.add_argument("--port", type=int, default=8001, help="Port for SSE transport (default: 8001)")

    args = parser.parse_args()

    transport = "sse" if args.sse else args.transport

    if transport == "stdio":
        # Run standard MCP stdio transport (no prints on stdout!)
        mcp.run(transport="stdio")
    else:
        print(f"Starting OrderFlow MCP Server with SSE transport on http://{args.host}:{args.port}")
        mcp.run(transport="sse", host=args.host, port=args.port)


if __name__ == "__main__":
    main()
