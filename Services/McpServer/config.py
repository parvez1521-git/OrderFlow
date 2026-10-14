"""
OrderFlow MCP Server Configuration
"""
import os
import httpx

# Upstream microservices endpoints
GATEWAY_URL = os.environ.get("ORDERFLOW_GATEWAY_URL", "http://localhost:6200/api").rstrip("/")
GATEWAY_USER = os.environ.get("ORDERFLOW_GATEWAY_USER", "orderflow-admin")
GATEWAY_PASS = os.environ.get("ORDERFLOW_GATEWAY_PASS", "orderflow-dev-password")

KNOWLEDGE_URL = os.environ.get("ORDERFLOW_KNOWLEDGE_URL", "http://localhost:8000/api").rstrip("/")

# Optional server-level API key for external AI clients
MCP_API_KEY = os.environ.get("ORDERFLOW_MCP_KEY", "").strip()

TIMEOUT_SECONDS = float(os.environ.get("ORDERFLOW_TIMEOUT", "10.0"))


def get_gateway_client() -> httpx.AsyncClient:
    """Creates an async HTTP client for the OrderFlow API Gateway with Basic Auth."""
    return httpx.AsyncClient(
        base_url=GATEWAY_URL,
        auth=(GATEWAY_USER, GATEWAY_PASS),
        timeout=TIMEOUT_SECONDS,
        headers={"Accept": "application/json"}
    )


def get_knowledge_client() -> httpx.AsyncClient:
    """Creates an async HTTP client for the KnowledgeService RAG API."""
    return httpx.AsyncClient(
        base_url=KNOWLEDGE_URL,
        timeout=TIMEOUT_SECONDS,
        headers={"Accept": "application/json"}
    )
