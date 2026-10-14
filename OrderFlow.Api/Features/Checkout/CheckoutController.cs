/* Customer checkout endpoint for Phases 6 and 7. */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Features.Auth;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Features.Checkout;

[ApiController]
[Route("api/checkout")]
[Authorize(Roles = "Customer")]
public sealed class CheckoutController(ICheckoutService checkout) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CheckoutResponse>> Checkout(CheckoutRequest request, CancellationToken ct)
    {
        try { return Ok(await checkout.CheckoutAsync(User.RequiredCustomerId(), request, ct)); }
        catch (BusinessRuleException ex) { return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest }); }
    }
}
