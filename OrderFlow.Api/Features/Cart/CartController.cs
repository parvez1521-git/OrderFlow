/* Customer shopping cart API for Phase 4. */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Features.Auth;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Features.Cart;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Customer")]
public sealed class CartController(ICartService cart) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<CartResponse>> Get(CancellationToken ct) => Ok(await cart.GetAsync(User.RequiredCustomerId(), ct));
    [HttpPost("items")] public async Task<ActionResult<CartResponse>> Add(CartItemRequest request, CancellationToken ct) => await Execute(() => cart.AddAsync(User.RequiredCustomerId(), request, ct));
    [HttpPut("items")] public async Task<ActionResult<CartResponse>> Update(CartItemRequest request, CancellationToken ct) => await Execute(() => cart.UpdateAsync(User.RequiredCustomerId(), request, ct));
    [HttpDelete("items/{productId:int}")] public async Task<ActionResult<CartResponse>> Remove(int productId, CancellationToken ct) => await Execute(() => cart.RemoveAsync(User.RequiredCustomerId(), productId, ct));
    [HttpDelete] public async Task<IActionResult> Clear(CancellationToken ct) { await cart.ClearAsync(User.RequiredCustomerId(), ct); return NoContent(); }
    private static async Task<ActionResult<CartResponse>> Execute(Func<Task<CartResponse>> action)
    {
        try { return await action(); }
        catch (BusinessRuleException ex) { return new BadRequestObjectResult(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest }); }
    }
}
