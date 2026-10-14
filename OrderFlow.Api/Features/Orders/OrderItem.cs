/* A line item belonging to an order. UnitPrice is stored as a historical price
 * snapshot rather than read from the product table when an order is viewed. */
namespace OrderFlow.Api.Models;

public sealed class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
