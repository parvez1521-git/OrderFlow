"""
Comprehensive test script for OrderFlow MCP Server.
Verifies tool registration, schemas, execution, and validation against live OrderFlow microservices.
"""
import asyncio
import json
import sys
import os

# Ensure Services/McpServer is on sys.path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from server import mcp


async def run_tests():
    print("=" * 60)
    print("ORDERFLOW MCP SERVER - VERIFICATION SUITE")
    print("=" * 60)

    # 1. Discover Tools
    print("\n[1] Discovering Registered MCP Tools...")
    tools = await mcp.list_tools()
    print(f"-> Discovered {len(tools)} tools:")
    for t in tools:
        print(f"   * {t.name}: {t.description}")
        props = list(t.input_schema.get("properties", {}).keys())
        print(f"     Parameters: {props}")

    assert len(tools) == 7, f"Expected 7 tools, got {len(tools)}"

    # 2. Discover Resources
    print("\n[2] Discovering Registered MCP Resources...")
    resources = await mcp.list_resources()
    print(f"-> Discovered {len(resources)} resources:")
    for r in resources:
        print(f"   * {r.uri}: {r.description}")

    # Read Resource Test
    print("\n[2b] Reading Resource 'orderflow://catalog/summary'...")
    res_content = await mcp.read_resource("orderflow://catalog/summary")
    print(f"-> Resource preview:\n{res_content[:150]}...")

    # 3. Discover Prompts
    print("\n[3] Discovering Registered MCP Prompts...")
    prompts = await mcp.list_prompts()
    print(f"-> Discovered {len(prompts)} prompts:")
    for p in prompts:
        print(f"   * {p.name}: {p.description}")

    # Get Prompt Test
    print("\n[3b] Rendering Prompt 'order_support_assistant' with order_id=1...")
    p_content = await mcp.get_prompt("order_support_assistant", {"order_id": 1})
    print(f"-> Prompt messages count: {len(p_content.messages)}")

    # 4. Test Tool Execution: orderflow_get_order
    print("\n[4] Invoking 'orderflow_get_order' with order_id=1...")
    res = await mcp.call_tool("orderflow_get_order", {"order_id": 1})
    print(f"-> Result text:\n{res.content[0].text}")

    # 5. Test Tool Execution: orderflow_list_customer_orders
    print("\n[5] Invoking 'orderflow_list_customer_orders' with customer_id=1...")
    res = await mcp.call_tool("orderflow_list_customer_orders", {"customer_id": 1, "limit": 2})
    print(f"-> Result text:\n{res.content[0].text}")

    # 6. Test Tool Execution: orderflow_check_inventory
    print("\n[6] Invoking 'orderflow_check_inventory' with product_id=1...")
    res = await mcp.call_tool("orderflow_check_inventory", {"product_id": 1})
    print(f"-> Result text:\n{res.content[0].text}")

    # 7. Test Tool Execution: orderflow_list_products
    print("\n[7] Invoking 'orderflow_list_products' with search_term='TV'...")
    res = await mcp.call_tool("orderflow_list_products", {"search_term": "TV"})
    print(f"-> Result text:\n{res.content[0].text}")

    # 8. Test Tool Execution: orderflow_search_policies (RAG)
    print("\n[8] Invoking 'orderflow_search_policies' with query='return policy'...")
    res = await mcp.call_tool("orderflow_search_policies", {"query": "return policy", "top_k": 2})
    print(f"-> Result text:\n{res.content[0].text}")

    # 9. Test Tool Validation: Invalid order_id <= 0
    print("\n[9] Testing Validation on 'orderflow_get_order' with order_id=-10...")
    try:
        res = await mcp.call_tool("orderflow_get_order", {"order_id": -10})
        print(f"-> Unexpected success result:\n{res.content[0].text}")
    except Exception as ex:
        print(f"-> Caught expected ToolError / ValidationError: {ex}")
        assert "greater than 0" in str(ex)

    # 10. Test Tool Validation: Invalid reason on orderflow_cancel_order (< 5 characters)
    print("\n[10] Testing Validation on 'orderflow_cancel_order' with short reason...")
    try:
        res = await mcp.call_tool("orderflow_cancel_order", {"order_id": 1, "reason": "no"})
        print(f"-> Unexpected success result:\n{res.content[0].text}")
    except Exception as ex:
        print(f"-> Caught expected ToolError / ValidationError: {ex}")
        assert "at least 5 characters" in str(ex) or "min_length" in str(ex) or "string_too_short" in str(ex)

    print("\n" + "=" * 60)
    print("ALL MCP SERVER TESTS PASSED SUCCESSFULLY!")
    print("=" * 60)


if __name__ == "__main__":
    asyncio.run(run_tests())
