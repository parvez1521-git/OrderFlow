/* Authentication request/response contracts shared by the portals. */
using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.Features.Auth;

public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record RegisterCustomerRequest([Required, MaxLength(200)] string Name, [Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
public sealed record AuthResponse(string Token, DateTime ExpiresAtUtc, string UserId, string Email, int? CustomerId, IReadOnlyCollection<string> Roles);
public sealed record CurrentUserResponse(string UserId, string Email, int? CustomerId, IReadOnlyCollection<string> Roles);
