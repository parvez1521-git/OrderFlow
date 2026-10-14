/* Product persistence model. Its price is the authoritative catalog price used
 * by the backend when it creates an order item. */
namespace OrderFlow.Api.Models;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
