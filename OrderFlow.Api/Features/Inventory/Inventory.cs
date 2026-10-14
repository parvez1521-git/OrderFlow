/* Inventory persistence model for Phase 5 stock tracking. */
namespace OrderFlow.Api.Models;

public sealed class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public Product Product { get; set; } = null!;
}
