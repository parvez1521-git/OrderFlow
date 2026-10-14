"""
OrderFlow MCP Server Tools Implementation.
Provides validated, authorized tools for AI clients to interact with OrderFlow services.
"""
from typing import Any, Dict, List, Optional
from pydantic import Field
from config import get_gateway_client, get_knowledge_client, MCP_API_KEY


async def orderflow_get_order(
    order_id: int = Field(gt=0, description="The unique positive integer ID of the order.")
) -> Dict[str, Any]:
    """Retrieve complete details for a specific order by its order ID, including items, total price, customer ID, and status."""
    if order_id <= 0:
        return {"error": "Validation error: order_id must be a positive integer greater than 0."}

    async with get_gateway_client() as client:
        try:
            resp = await client.get(f"orders/{order_id}")
            if resp.status_code == 404:
                return {"error": f"Order #{order_id} was not found."}
            resp.raise_for_status()
            data = resp.json()
            return {
                "order_id": data.get("id"),
                "customer_id": data.get("customerId"),
                "status": data.get("status"),
                "order_date_utc": data.get("orderDateUtc"),
                "total": data.get("total"),
                "items": data.get("items", [])
            }
        except Exception as ex:
            return {"error": f"Failed to retrieve order #{order_id}: {str(ex)}"}


async def orderflow_list_customer_orders(
    customer_id: int = Field(gt=0, description="The numeric customer ID whose orders to retrieve."),
    limit: int = Field(default=10, ge=1, le=50, description="Maximum number of orders to return (1-50, default 10).")
) -> Dict[str, Any]:
    """List orders placed by a specific customer, sorted with the most recent orders first."""
    if customer_id <= 0:
        return {"error": "Validation error: customer_id must be greater than 0."}
    if limit < 1 or limit > 50:
        return {"error": "Validation error: limit must be between 1 and 50."}

    async with get_gateway_client() as client:
        try:
            resp = await client.get("orders")
            resp.raise_for_status()
            all_orders = resp.json()
            customer_orders = [o for o in all_orders if o.get("customerId") == customer_id]
            customer_orders.sort(key=lambda o: o.get("id", 0), reverse=True)
            return {
                "customer_id": customer_id,
                "total_found": len(customer_orders),
                "orders": customer_orders[:limit]
            }
        except Exception as ex:
            return {"error": f"Failed to list orders for customer #{customer_id}: {str(ex)}"}


async def orderflow_check_inventory(
    product_id: int = Field(gt=0, description="The unique numeric product ID to inspect stock levels for.")
) -> Dict[str, Any]:
    """Check live stock availability and current inventory quantity for a product."""
    if product_id <= 0:
        return {"error": "Validation error: product_id must be a positive integer."}

    async with get_gateway_client() as client:
        try:
            resp = await client.get(f"inventory/{product_id}")
            if resp.status_code == 404:
                return {"error": f"No inventory record found for product #{product_id}."}
            resp.raise_for_status()
            inv = resp.json()
            qty = inv.get("availableQuantity", 0)

            if qty <= 0:
                stock_status = "OUT_OF_STOCK"
            elif qty <= 5:
                stock_status = "LOW_STOCK"
            else:
                stock_status = "IN_STOCK"

            return {
                "product_id": product_id,
                "available_quantity": qty,
                "stock_status": stock_status
            }
        except Exception as ex:
            return {"error": f"Failed to check inventory for product #{product_id}: {str(ex)}"}


async def orderflow_list_products(
    search_term: str = Field(default="", description="Optional search term to filter products by name."),
    limit: int = Field(default=20, ge=1, le=50, description="Maximum number of products to return (1-50, default 20).")
) -> Dict[str, Any]:
    """Browse or search available catalog products with their prices and IDs."""
    if limit < 1 or limit > 50:
        return {"error": "Validation error: limit must be between 1 and 50."}

    async with get_gateway_client() as client:
        try:
            resp = await client.get("products")
            resp.raise_for_status()
            products = resp.json()

            if search_term.strip():
                term = search_term.strip().lower()
                products = [p for p in products if term in p.get("name", "").lower()]

            return {
                "count": len(products[:limit]),
                "products": products[:limit]
            }
        except Exception as ex:
            return {"error": f"Failed to fetch products: {str(ex)}"}


async def orderflow_cancel_order(
    order_id: int = Field(gt=0, description="The unique ID of the order to cancel."),
    reason: str = Field(min_length=5, description="The required cancellation reason provided by customer or agent (at least 5 characters)."),
    api_key: Optional[str] = Field(default="", description="Optional MCP API key if required by server security configuration.")
) -> Dict[str, Any]:
    """Request cancellation of an active order. Validates that the order exists, has not already been cancelled or completed, and requires a valid reason."""
    # 1. Authorization check
    if MCP_API_KEY and api_key != MCP_API_KEY:
        return {
            "error": "Unauthorized: A valid API key is required to execute order cancellation."
        }

    # 2. Input validation
    if order_id <= 0:
        return {"error": "Validation error: order_id must be a positive integer."}
    if not reason or len(reason.strip()) < 5:
        return {"error": "Validation error: cancellation reason must be at least 5 characters long."}

    async with get_gateway_client() as client:
        try:
            # 3. Check existing order status before cancelling
            get_resp = await client.get(f"orders/{order_id}")
            if get_resp.status_code == 404:
                return {"error": f"Order #{order_id} does not exist."}
            get_resp.raise_for_status()
            order = get_resp.json()

            status = str(order.get("status", "")).strip()
            if status in ["Cancelled", "Canceled"]:
                return {"error": f"Order #{order_id} is already cancelled."}
            if status in ["Shipped", "Delivered", "Completed"]:
                return {
                    "error": f"Order #{order_id} cannot be cancelled automatically because it has status '{status}'. Please contact customer support."
                }

            # 4. Perform cancellation
            cancel_resp = await client.post(f"orders/{order_id}/cancel")
            cancel_resp.raise_for_status()
            updated = cancel_resp.json()

            return {
                "success": True,
                "order_id": order_id,
                "previous_status": status,
                "current_status": updated.get("status", "Cancelled"),
                "reason": reason.strip(),
                "message": f"Order #{order_id} has been successfully cancelled."
            }
        except Exception as ex:
            return {"error": f"Failed to cancel order #{order_id}: {str(ex)}"}


async def orderflow_search_policies(
    query: str = Field(min_length=2, description="The store policy topic to search for (e.g., returns, refund window, delivery, warranty)."),
    top_k: int = Field(default=3, ge=1, le=10, description="Number of top policy document matches to retrieve (1-10, default 3).")
) -> Dict[str, Any]:
    """Search store knowledge base and policies (returns, warranties, shipping, refund conditions) using RAG similarity matching."""
    if not query or len(query.strip()) < 2:
        return {"error": "Validation error: query must be at least 2 characters long."}
    if top_k < 1 or top_k > 10:
        return {"error": "Validation error: top_k must be between 1 and 10."}

    async with get_knowledge_client() as client:
        try:
            resp = await client.post("documents/search", json={"query": query.strip(), "top_k": top_k})
            resp.raise_for_status()
            return resp.json()
        except Exception as ex:
            return {"error": f"Failed to query knowledge policies: {str(ex)}"}


async def orderflow_place_order(
    product_id: int = Field(gt=0, description="The numeric ID of the product to purchase (e.g., 1 for TV, 2 for Microwave)."),
    quantity: int = Field(default=1, ge=1, le=10, description="Quantity to purchase (1-10, default 1)."),
    customer_id: int = Field(default=1, gt=0, description="The customer ID placing the order (default 1)."),
    customer_email: str = Field(default="customer@oms.local", description="Customer email for confirmation.")
) -> Dict[str, Any]:
    """Place a new order in OrderFlow. Checks inventory stock and executes the checkout Saga across microservices."""
    import uuid

    if product_id <= 0:
        return {"error": "Validation error: product_id must be a positive integer."}
    if quantity < 1 or quantity > 10:
        return {"error": "Validation error: quantity must be between 1 and 10."}
    if customer_id <= 0:
        return {"error": "Validation error: customer_id must be a positive integer."}

    async with get_gateway_client() as client:
        try:
            # 1. Check stock
            inv_resp = await client.get(f"inventory/{product_id}")
            if inv_resp.status_code == 404:
                return {"error": f"Product #{product_id} has no inventory record."}
            inv_resp.raise_for_status()
            available = inv_resp.json().get("availableQuantity", 0)
            if available < quantity:
                return {"error": f"Insufficient stock: product #{product_id} only has {available} units available (requested {quantity})."}

            # 2. Execute checkout saga
            payload = {
                "customerId": customer_id,
                "customerEmail": customer_email,
                "paymentMode": "success",
                "idempotencyKey": str(uuid.uuid4()),
                "items": [{"productId": product_id, "quantity": quantity}]
            }
            checkout_resp = await client.post("checkout", json=payload)
            checkout_resp.raise_for_status()
            result = checkout_resp.json()

            return {
                "success": True,
                "order_id": result.get("orderId"),
                "status": result.get("orderStatus"),
                "payment_status": result.get("paymentStatus"),
                "total": result.get("total"),
                "customer_id": customer_id,
                "product_id": product_id,
                "quantity": quantity,
                "message": f"Order #{result.get('orderId')} placed successfully for {quantity} unit(s) of product #{product_id}!"
            }
        except Exception as ex:
            return {"error": f"Failed to place order: {str(ex)}"}

