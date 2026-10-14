/* Small authorization helper for converting JWT customerId claims to integers. */
using System.Security.Claims;

namespace OrderFlow.Api.Features.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int RequiredCustomerId(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue("customerId"), out var customerId) ? customerId : throw new UnauthorizedAccessException("Customer account is not linked to a customer record.");
}
