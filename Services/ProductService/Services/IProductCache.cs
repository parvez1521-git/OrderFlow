/*
 * Product cache abstraction.
 * Controllers depend on this interface so Redis can be unavailable without
 * breaking normal SQL Server product reads.
 */
using OrderFlow.ProductService.Models;

namespace OrderFlow.ProductService.Services;

public interface IProductCache
{
    Task<IReadOnlyList<Product>?> GetProductsAsync();

    Task SetProductsAsync(IReadOnlyList<Product> products);

    Task InvalidateProductsAsync();
}
