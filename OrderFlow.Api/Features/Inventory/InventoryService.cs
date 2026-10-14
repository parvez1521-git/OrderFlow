/* Inventory service owns stock checks, reservation, and release rules. */
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Features.Inventory;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<InventoryResponse?> GetByProductIdAsync(int productId, CancellationToken cancellationToken);
    Task<InventoryResponse> UpsertAsync(UpsertInventoryRequest request, CancellationToken cancellationToken);
    Task ReserveStockAsync(IReadOnlyCollection<(int ProductId, int Quantity)> items, CancellationToken cancellationToken);
    Task ReleaseStockAsync(IReadOnlyCollection<(int ProductId, int Quantity)> items, CancellationToken cancellationToken);
}

public sealed class InventoryService(OrderFlowDbContext db) : IInventoryService
{
    public async Task<IReadOnlyList<InventoryResponse>> GetAllAsync(CancellationToken ct) =>
        await Query().OrderBy(x => x.ProductId).Select(x => ToResponse(x)).ToListAsync(ct);

    public Task<InventoryResponse?> GetByProductIdAsync(int productId, CancellationToken ct) =>
        Query().Where(x => x.ProductId == productId).Select(x => ToResponse(x)).SingleOrDefaultAsync(ct);

    public async Task<InventoryResponse> UpsertAsync(UpsertInventoryRequest request, CancellationToken ct)
    {
        if (!await db.Products.AnyAsync(x => x.Id == request.ProductId, ct)) throw new BusinessRuleException("Product was not found.");
        var inventory = await db.Inventories.SingleOrDefaultAsync(x => x.ProductId == request.ProductId, ct);
        if (inventory is null)
        {
            inventory = new Models.Inventory { ProductId = request.ProductId, AvailableQuantity = request.AvailableQuantity };
            db.Inventories.Add(inventory);
        }
        else inventory.AvailableQuantity = request.AvailableQuantity;
        await db.SaveChangesAsync(ct);
        return (await GetByProductIdAsync(request.ProductId, ct))!;
    }

    public async Task ReserveStockAsync(IReadOnlyCollection<(int ProductId, int Quantity)> items, CancellationToken ct)
    {
        var ids = items.Select(x => x.ProductId).Distinct().ToArray();
        var inventory = await db.Inventories.Where(x => ids.Contains(x.ProductId)).ToDictionaryAsync(x => x.ProductId, ct);
        foreach (var item in items)
        {
            if (!inventory.TryGetValue(item.ProductId, out var row)) throw new BusinessRuleException($"Inventory is not configured for product {item.ProductId}.");
            if (row.AvailableQuantity < item.Quantity) throw new BusinessRuleException($"Insufficient inventory for product {item.ProductId}.");
        }
        foreach (var item in items) inventory[item.ProductId].AvailableQuantity -= item.Quantity;
    }

    public async Task ReleaseStockAsync(IReadOnlyCollection<(int ProductId, int Quantity)> items, CancellationToken ct)
    {
        var ids = items.Select(x => x.ProductId).Distinct().ToArray();
        var inventory = await db.Inventories.Where(x => ids.Contains(x.ProductId)).ToDictionaryAsync(x => x.ProductId, ct);
        foreach (var item in items)
            if (inventory.TryGetValue(item.ProductId, out var row)) row.AvailableQuantity += item.Quantity;
    }

    private IQueryable<Models.Inventory> Query() => db.Inventories.AsNoTracking().Include(x => x.Product);
    private static InventoryResponse ToResponse(Models.Inventory inventory) => new(inventory.Id, inventory.ProductId, inventory.Product.Name, inventory.AvailableQuantity);
}
