/* Typed HTTP client for synchronous calls from OrderService to ProductService. */
using System.Net.Http.Json;

namespace OrderFlow.OrderService.Clients;

public interface IProductCatalogClient
{
    Task<ProductSnapshot> GetProductAsync(int productId, CancellationToken cancellationToken);
}

public sealed class ProductCatalogClient(HttpClient http) : IProductCatalogClient
{
    public async Task<ProductSnapshot> GetProductAsync(int productId, CancellationToken ct)
    {
        var response = await http.GetAsync($"/api/products/{productId}", ct);
        if (!response.IsSuccessStatusCode) throw new DownstreamServiceException("ProductService", $"Product {productId} was not found.");
        return (await response.Content.ReadFromJsonAsync<ProductSnapshot>(cancellationToken: ct))!;
    }
}

public sealed record ProductSnapshot(int Id, string Name, decimal Price);
