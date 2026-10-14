/* Inventory item owned by the Inventory microservice. ProductId is external. */
namespace OrderFlow.InventoryService.Models;

public sealed class InventoryItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int AvailableQuantity { get; set; }
}
