using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Application.Models;
using CustomerPayments.Api.DTOs;
using CustomerPayments.Api.Options;
using Microsoft.Extensions.Options;

namespace CustomerPayments.Api.Application.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly DemoUserOptions _demoUserOptions;

    public AuthenticationService(
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions,
        IOptions<DemoUserOptions> demoUserOptions)
    {
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
        _demoUserOptions = demoUserOptions.Value;
    }

    public Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsValidDemoUser(request))
        {
            return Task.FromResult<LoginResponse?>(null);
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        var user = new UserTokenData(
            UserId: _demoUserOptions.UserId,
            Email: _demoUserOptions.Email,
            Role: _demoUserOptions.Role);

        var accessToken = _jwtTokenService.CreateToken(user, expiresAtUtc);

        var response = new LoginResponse(accessToken, expiresAtUtc);

        return Task.FromResult<LoginResponse?>(response);
    }

    private bool IsValidDemoUser(LoginRequest request)
    {
        return request.Email.Equals(
                   _demoUserOptions.Email,
                   StringComparison.OrdinalIgnoreCase)
               && request.Password == _demoUserOptions.Password;
    }
}