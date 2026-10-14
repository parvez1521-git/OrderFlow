/* Typed HTTP client for synchronous stock calls to InventoryService. */
using System.Net.Http.Json;

namespace OrderFlow.OrderService.Clients;

public interface IInventoryClient
{
    Task ReserveAsync(int productId, int quantity, CancellationToken cancellationToken);
    Task ReleaseAsync(int productId, int quantity, CancellationToken cancellationToken);
}

public sealed class InventoryClient(HttpClient http) : IInventoryClient
{
    public async Task ReserveAsync(int productId, int quantity, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/inventory/reserve", new StockRequest(productId, quantity), ct);
        if (!response.IsSuccessStatusCode) throw new DownstreamServiceException("InventoryService", await response.Content.ReadAsStringAsync(ct));
    }

    public async Task ReleaseAsync(int productId, int quantity, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/inventory/release", new StockRequest(productId, quantity), ct);
        if (!response.IsSuccessStatusCode) throw new DownstreamServiceException("InventoryService", await response.Content.ReadAsStringAsync(ct));
    }
}

public sealed record StockRequest(int ProductId, int Quantity);
