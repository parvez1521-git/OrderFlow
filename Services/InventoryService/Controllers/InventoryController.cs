/* Controller for inventory and stock reservation endpoints. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.InventoryService.Data;
using OrderFlow.InventoryService.Dtos;
using OrderFlow.InventoryService.Models;

namespace OrderFlow.InventoryService.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(InventoryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<InventoryItem>> GetAll() => await db.Inventory.AsNoTracking().OrderBy(x => x.ProductId).ToListAsync();

    [HttpGet("{productId:int}")]
    public async Task<ActionResult<InventoryItem>> GetByProductId(int productId) => await db.Inventory.SingleOrDefaultAsync(x => x.ProductId == productId) is { } row ? Ok(row) : NotFound();

    [HttpPut]
    public async Task<ActionResult<InventoryItem>> Upsert(InventoryRequest request)
    {
        var row = await db.Inventory.SingleOrDefaultAsync(x => x.ProductId == request.ProductId);
        if (row is null)
        {
            row = new InventoryItem { ProductId = request.ProductId, AvailableQuantity = request.AvailableQuantity };
            db.Inventory.Add(row);
        }
        else row.AvailableQuantity = request.AvailableQuantity;
        await db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<InventoryItem>> Reserve(StockRequest request)
    {
        var row = await db.Inventory.SingleOrDefaultAsync(x => x.ProductId == request.ProductId);
        if (row is null) return NotFound("Inventory is not configured for this product.");
        if (row.AvailableQuantity < request.Quantity) return BadRequest("Insufficient inventory.");
        row.AvailableQuantity -= request.Quantity;
        await db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpPost("release")]
    public async Task<ActionResult<InventoryItem>> Release(StockRequest request)
    {
        var row = await db.Inventory.SingleOrDefaultAsync(x => x.ProductId == request.ProductId);
        if (row is null)
        {
            row = new InventoryItem { ProductId = request.ProductId, AvailableQuantity = request.Quantity };
            db.Inventory.Add(row);
        }
        else row.AvailableQuantity += request.Quantity;
        await db.SaveChangesAsync();
        return Ok(row);
    }
}
