/*
 * Identity user model for OrderFlow login accounts.
 * Customer users are linked to a Customer record so authorization can enforce
 * "customers can only see their own orders".
 */
using Microsoft.AspNetCore.Identity;

namespace OrderFlow.Api.Features.Auth;

public sealed class ApplicationUser : IdentityUser
{
    public int? CustomerId { get; set; }
}
