using System.ComponentModel;
using System.Net.Http.Json;

namespace OrderFlow.AiService.Tools;

public sealed class OrderFlowTools(IHttpClientFactory httpClientFactory, ILogger<OrderFlowTools> logger)
{
    private HttpClient CreateGatewayClient() => httpClientFactory.CreateClient("Gateway");

    [Description("Gets all orders for a specific customer ID.")]
    public async Task<string> GetCustomerOrdersAsync(
        [Description("The numeric ID of the customer whose orders to fetch")] int customerId)
    {
        try
        {
            var client = CreateGatewayClient();
            var orders = await client.GetFromJsonAsync<List<OrderSummary>>("orders");
            if (orders is null || orders.Count == 0) return "No orders found.";

            var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();
            if (customerOrders.Count == 0) return $"No orders found for customer {customerId}.";

            return System.Text.Json.JsonSerializer.Serialize(customerOrders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get customer orders");
            return "Unable to retrieve orders at this time.";
        }
    }

    [Description("Gets the latest or most recent order placed in the system, optionally filtered by customer ID.")]
    public async Task<string> GetLatestOrderAsync(
        [Description("Optional customer ID to filter by")] int? customerId = null)
    {
        try
        {
            var client = CreateGatewayClient();
            var orders = await client.GetFromJsonAsync<List<OrderSummary>>("orders");
            if (orders is null || orders.Count == 0) return "No orders found in the system.";

            var matching = customerId.HasValue && customerId.Value > 0
                ? orders.Where(o => o.CustomerId == customerId.Value).ToList()
                : orders;

            var latest = matching.OrderByDescending(o => o.Id).FirstOrDefault();
            if (latest is null) return $"No orders found for customer {customerId}.";

            return System.Text.Json.JsonSerializer.Serialize(latest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get latest order");
            return "Unable to retrieve the latest order.";
        }
    }

    [Description("Gets details, items, total, and status of a specific order by its order ID.")]
    public async Task<string> GetOrderDetailsAsync(
        [Description("The numeric order ID")] int orderId)
    {
        try
        {
            var client = CreateGatewayClient();
            var order = await client.GetFromJsonAsync<OrderSummary>($"orders/{orderId}");
            if (order is null) return $"Order #{orderId} not found.";
            return System.Text.Json.JsonSerializer.Serialize(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get order #{OrderId}", orderId);
            return $"Order #{orderId} was not found or could not be retrieved.";
        }
    }

    [Description("Checks current available inventory stock for a product ID.")]
    public async Task<string> CheckInventoryAsync(
        [Description("The numeric product ID")] int productId)
    {
        try
        {
            var client = CreateGatewayClient();
            var stock = await client.GetFromJsonAsync<InventoryRecord>($"inventory/{productId}");
            return stock is not null ? $"Product {productId} has {stock.AvailableQuantity} units in stock." : $"No inventory record found for product {productId}.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check inventory for product #{ProductId}", productId);
            return "Unable to check inventory.";
        }
    }

    [Description("Lists available products with names and prices in the catalog.")]
    public async Task<string> ListCatalogProductsAsync()
    {
        try
        {
            var client = CreateGatewayClient();
            var products = await client.GetFromJsonAsync<List<ProductItem>>("products");
            return System.Text.Json.JsonSerializer.Serialize(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list products");
            return "Unable to list catalog products.";
        }
    }

    [Description("Searches the store knowledge base and policy documents for returns, refunds, shipping, delivery, warranties, and cancellations.")]
    public async Task<string> SearchStorePoliciesAsync(
        [Description("The policy topic or customer question to search")] string query)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://localhost:8000/api/") };
            var response = await client.PostAsJsonAsync("documents/search", new { query, top_k = 3 });
            if (!response.IsSuccessStatusCode) return "No matching policy documents found.";

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query KnowledgeService RAG");
            return "Unable to search store policies at this time.";
        }
    }
}

public sealed record OrderSummary(int Id, int CustomerId, DateTime OrderDateUtc, string Status, decimal Total, List<OrderItemDetail> Items);
public sealed record OrderItemDetail(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public sealed record InventoryRecord(int ProductId, int AvailableQuantity);
public sealed record ProductItem(int Id, string Name, decimal Price);
