/* Admin API for Phase 5 inventory management. */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Features.Inventory;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "Admin,OrderManager")]
public sealed class InventoryController(IInventoryService inventory) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<InventoryResponse>> GetAll(CancellationToken ct) => inventory.GetAllAsync(ct);
    [HttpGet("{productId:int}")] public async Task<ActionResult<InventoryResponse>> GetByProduct(int productId, CancellationToken ct) => (await inventory.GetByProductIdAsync(productId, ct)) is { } row ? Ok(row) : NotFound();
    [HttpPut] public async Task<ActionResult<InventoryResponse>> Upsert(UpsertInventoryRequest request, CancellationToken ct)
    {
        try { return Ok(await inventory.UpsertAsync(request, ct)); }
        catch (BusinessRuleException ex) { return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest }); }
    }
}
