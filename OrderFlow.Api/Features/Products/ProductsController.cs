/* HTTP endpoint adapter for product CRUD. It is deliberately thin so the
 * ProductService remains reusable and independently testable. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderFlow.Api.Services;
namespace OrderFlow.Api.Features.Products;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [Authorize]
    [HttpGet] public Task<IReadOnlyList<ProductResponse>> GetAll(CancellationToken ct) => products.GetAllAsync(ct);
    [Authorize]
    [HttpGet("{id:int}")] public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct) => (await products.GetByIdAsync(id, ct)) is { } product ? Ok(product) : NotFound();
    [Authorize(Roles = "Admin")]
    [HttpPost] public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct) { var product = await products.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { product.Id }, product); }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")] public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request, CancellationToken ct) => (await products.UpdateAsync(id, request, ct)) is { } product ? Ok(product) : NotFound();
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try { return await products.DeleteAsync(id, ct) ? NoContent() : NotFound(); }
        catch (BusinessRuleException ex) { return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict }); }
    }
}
