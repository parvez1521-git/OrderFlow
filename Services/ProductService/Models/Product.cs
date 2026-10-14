/* Product entity owned by the Product microservice. */
namespace OrderFlow.ProductService.Models;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
