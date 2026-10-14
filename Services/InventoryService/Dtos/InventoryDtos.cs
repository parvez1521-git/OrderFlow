/* Inventory request contracts for this microservice. */
namespace OrderFlow.InventoryService.Dtos;

public sealed record InventoryRequest(int ProductId, int AvailableQuantity);
public sealed record StockRequest(int ProductId, int Quantity);
