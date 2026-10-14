/* Login/register endpoints for Phase 3 authentication. */
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(UserManager<ApplicationUser> users, OrderFlowDbContext db, IJwtTokenService tokens) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register-customer")]
    public async Task<ActionResult<AuthResponse>> RegisterCustomer(RegisterCustomerRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.FindByEmailAsync(email) is not null) return Conflict(new ProblemDetails { Title = "A login with this email already exists." });

        var customer = new Customer { Name = request.Name.Trim(), Email = email };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);

        var user = new ApplicationUser { UserName = email, Email = email, CustomerId = customer.Id };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            db.Customers.Remove(customer);
            await db.SaveChangesAsync(ct);
            return BadRequest(new ProblemDetails { Title = string.Join(" ", result.Errors.Select(error => error.Description)) });
        }

        await users.AddToRoleAsync(user, "Customer");
        return Ok(await tokens.CreateTokenAsync(user, ct));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || !await users.CheckPasswordAsync(user, request.Password)) return Unauthorized(new ProblemDetails { Title = "Invalid email or password." });
        return Ok(await tokens.CreateTokenAsync(user, ct));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> Me()
    {
        var roles = User.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value).ToArray();
        var customerId = int.TryParse(User.FindFirstValue("customerId"), out var parsed) ? parsed : (int?)null;
        return new CurrentUserResponse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, User.FindFirstValue(ClaimTypes.Email) ?? string.Empty, customerId, roles);
    }
}
