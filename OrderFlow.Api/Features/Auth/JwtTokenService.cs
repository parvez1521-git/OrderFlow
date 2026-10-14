/* JWT creation stays behind a small service so controllers do not know token internals. */
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace OrderFlow.Api.Features.Auth;

public interface IJwtTokenService
{
    Task<AuthResponse> CreateTokenAsync(ApplicationUser user, CancellationToken cancellationToken);
}

public sealed class JwtTokenService(IConfiguration configuration, UserManager<ApplicationUser> users) : IJwtTokenService
{
    public async Task<AuthResponse> CreateTokenAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = await users.GetRolesAsync(user);
        var expires = DateTime.UtcNow.AddMinutes(configuration.GetValue("Jwt:Minutes", 60));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };
        if (user.CustomerId is int customerId) claims.Add(new("customerId", customerId.ToString()));
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, user.Id, user.Email ?? string.Empty, user.CustomerId, roles.ToArray());
    }
}
