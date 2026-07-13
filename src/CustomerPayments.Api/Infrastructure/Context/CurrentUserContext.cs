using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CustomerPayments.Api.Application.Interfaces;

namespace CustomerPayments.Api.Infrastructure.Context;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        GetClaimValue(JwtRegisteredClaimNames.Sub)
        ?? GetClaimValue(ClaimTypes.NameIdentifier);

    public string? Email =>
        GetClaimValue(JwtRegisteredClaimNames.Email)
        ?? GetClaimValue(ClaimTypes.Email);

    public string? Role =>
        GetClaimValue(ClaimTypes.Role);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    private string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor
            .HttpContext?
            .User
            .FindFirst(claimType)?
            .Value;
    }
}