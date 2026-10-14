/* HTTP endpoint adapter for order CRUD plus Phase 3 authorization rules. */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Features.Auth;
using OrderFlow.Api.Services;
namespace OrderFlow.Api.Features.Orders;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<OrderResponse>> GetAll(CancellationToken ct) => orders.GetAllAsync(CustomerScope(), ct);
    [HttpGet("{id:int}")] public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken ct) => (await orders.GetByIdAsync(id, CustomerScope(), ct)) is { } order ? Ok(order) : NotFound();
    [HttpPost] public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        if (User.IsInRole("Customer")) request = request with { CustomerId = User.RequiredCustomerId() };
        try { var order = await orders.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { order.Id }, order); }
        catch (BusinessRuleException ex) { return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest }); }
    }
    [Authorize(Roles = "Admin,OrderManager")]
    [HttpPut("{id:int}")] public async Task<ActionResult<OrderResponse>> Update(int id, UpdateOrderRequest request, CancellationToken ct)
    {
        try { return (await orders.UpdateAsync(id, request, ct)) is { } order ? Ok(order) : NotFound(); }
        catch (BusinessRuleException ex) { return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest }); }
    }
    [Authorize(Roles = "Admin,OrderManager")]
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await orders.DeleteAsync(id, ct) ? NoContent() : NotFound();
    private int? CustomerScope() => User.IsInRole("Customer") ? User.RequiredCustomerId() : null;
}
