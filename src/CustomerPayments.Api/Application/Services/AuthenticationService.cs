using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Application.Models;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.DTOs;
using CustomerPayments.Api.Interfaces.Repositories;
using CustomerPayments.Api.Interfaces.Services;
using CustomerPayments.Api.Options;
using Microsoft.Extensions.Options;

namespace CustomerPayments.Api.Application.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly DemoUserOptions _demoUserOptions;

    public AuthenticationService(
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtOptions> jwtOptions,
        IOptions<DemoUserOptions> demoUserOptions)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtOptions = jwtOptions.Value;
        _demoUserOptions = demoUserOptions.Value;
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsValidDemoUser(request))
        {
            return null;
        }

        var user = CreateDemoUserTokenData();

        return await CreateTokenPairAsync(
            user,
            cancellationToken);
    }

    public async Task<LoginResponse?> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var currentTokenHash =
            _refreshTokenService.ComputeHash(
                request.RefreshToken);

        var storedToken =
            await _refreshTokenRepository.GetByTokenHashAsync(
                currentTokenHash,
                cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return null;
        }

        var user = CreateDemoUserTokenData();

        var newRefreshTokenValue =
            _refreshTokenService.GenerateToken();

        var newRefreshTokenHash =
            _refreshTokenService.ComputeHash(
                newRefreshTokenValue);

        var now = DateTime.UtcNow;

        storedToken.RevokedAtUtc = now;
        storedToken.ReplacedByTokenHash =
            newRefreshTokenHash;

        var accessTokenExpiresAtUtc =
            now.AddMinutes(_jwtOptions.ExpirationMinutes);

        var refreshTokenExpiresAtUtc =
            now.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = storedToken.UserId,
            TokenHash = newRefreshTokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshTokenExpiresAtUtc
        };

        await _refreshTokenRepository.AddAsync(
            newRefreshToken,
            cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);

        var accessToken = _jwtTokenService.CreateToken(
            user,
            accessTokenExpiresAtUtc);

        return new LoginResponse(
            accessToken,
            accessTokenExpiresAtUtc,
            newRefreshTokenValue,
            refreshTokenExpiresAtUtc);
    }

    public async Task<bool> RevokeAsync(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var tokenHash =
            _refreshTokenService.ComputeHash(
                request.RefreshToken);

        var storedToken =
            await _refreshTokenRepository.GetByTokenHashAsync(
                tokenHash,
                cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return false;
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private bool IsValidDemoUser(LoginRequest request)
    {
        return request.Email.Equals(
                   _demoUserOptions.Email,
                   StringComparison.OrdinalIgnoreCase)
               && request.Password == _demoUserOptions.Password;
    }

    private async Task<LoginResponse> CreateTokenPairAsync(
        UserTokenData user,
        CancellationToken cancellationToken)
    {
        var accessTokenExpiresAtUtc =
            DateTime.UtcNow.AddMinutes(
                _jwtOptions.ExpirationMinutes);

        var accessToken = _jwtTokenService.CreateToken(
            user,
            accessTokenExpiresAtUtc);

        var refreshTokenValue =
            _refreshTokenService.GenerateToken();

        var refreshTokenHash =
            _refreshTokenService.ComputeHash(
                refreshTokenValue);

        var refreshTokenExpiresAtUtc =
            DateTime.UtcNow.AddDays(
                _jwtOptions.RefreshTokenExpirationDays);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshTokenExpiresAtUtc
        };

        await _refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);

        return new LoginResponse(
            accessToken,
            accessTokenExpiresAtUtc,
            refreshTokenValue,
            refreshTokenExpiresAtUtc);
    }

    private UserTokenData CreateDemoUserTokenData()
    {
        return new UserTokenData(
            UserId: _demoUserOptions.UserId,
            Email: _demoUserOptions.Email,
            Role: _demoUserOptions.Role);
    }
}