/* Inventory request/response contracts. */
using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.Features.Inventory;

public sealed record InventoryResponse(int Id, int ProductId, string ProductName, int AvailableQuantity);
public sealed record UpsertInventoryRequest([Range(1, int.MaxValue)] int ProductId, [Range(0, int.MaxValue)] int AvailableQuantity);
public sealed record StockAdjustmentRequest([Range(1, int.MaxValue)] int ProductId, [Range(1, int.MaxValue)] int Quantity);
