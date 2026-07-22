using System.Net;
using System.Net.Http.Json;
using CustomerPayments.Api.Data;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.Interfaces.Services;
using CustomerPayments.IntegrationTests;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerPayments.Api.IntegrationTests;

public sealed class AuthenticationRefreshTokenTests
    : IClassFixture<CustomerPaymentsApiFactory>
{
    private const string LoginEndpoint =
        "/api/v1/auth/login";

    private const string RefreshEndpoint =
        "/api/v1/auth/refresh";

    private const string RevokeEndpoint =
        "/api/v1/auth/revoke";

    /*
     * Estos valores deben coincidir con DemoUser
     * dentro de appsettings.Testing.json.
     */
    private const string DemoEmail =
        "demo@customerpayments.com";

    private const string DemoPassword =
        "DemoPassword123!";

    private readonly CustomerPaymentsApiFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationRefreshTokenTests(
        CustomerPaymentsApiFactory factory)
    {
        _factory = factory;

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ShouldReturnAccessAndRefreshTokens()
    {
        // Arrange
        var request = CreateLoginRequest();

        // Act
        var response = await _client.PostAsJsonAsync(
            LoginEndpoint,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<TokenPairResponse>();

        Assert.NotNull(result);

        Assert.False(
            string.IsNullOrWhiteSpace(result.AccessToken));

        Assert.False(
            string.IsNullOrWhiteSpace(result.RefreshToken));

        Assert.True(
            result.AccessTokenExpiresAtUtc > DateTime.UtcNow);

        Assert.True(
            result.RefreshTokenExpiresAtUtc > DateTime.UtcNow);

        Assert.True(
            result.RefreshTokenExpiresAtUtc
            > result.AccessTokenExpiresAtUtc);
    }

    [Fact]
    public async Task Refresh_ShouldReturnNewTokenPair_WhenTokenIsValid()
    {
        // Arrange
        var originalTokenPair =
            await LoginAsync();

        var request = new RefreshTokenRequest(
            originalTokenPair.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync(
            RefreshEndpoint,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var refreshedTokenPair =
            await response.Content
                .ReadFromJsonAsync<TokenPairResponse>();

        Assert.NotNull(refreshedTokenPair);

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshedTokenPair.AccessToken));

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshedTokenPair.RefreshToken));

        Assert.NotEqual(
            originalTokenPair.RefreshToken,
            refreshedTokenPair.RefreshToken);

        Assert.NotEqual(
            originalTokenPair.AccessToken,
            refreshedTokenPair.AccessToken);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenTokenWasAlreadyUsed()
    {
        // Arrange
        var originalTokenPair =
            await LoginAsync();

        var request = new RefreshTokenRequest(
            originalTokenPair.RefreshToken);

        var firstRefreshResponse =
            await _client.PostAsJsonAsync(
                RefreshEndpoint,
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            firstRefreshResponse.StatusCode);

        // Act
        var secondRefreshResponse =
            await _client.PostAsJsonAsync(
                RefreshEndpoint,
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            secondRefreshResponse.StatusCode);
    }

    [Fact]
    public async Task Revoke_ShouldInvalidateRefreshToken()
    {
        // Arrange
        var tokenPair =
            await LoginAsync();

        var revokeRequest = new RevokeRefreshTokenRequest(
            tokenPair.RefreshToken);

        var revokeResponse =
            await _client.PostAsJsonAsync(
                RevokeEndpoint,
                revokeRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            revokeResponse.StatusCode);

        var refreshRequest = new RefreshTokenRequest(
            tokenPair.RefreshToken);

        // Act
        var refreshResponse =
            await _client.PostAsJsonAsync(
                RefreshEndpoint,
                refreshRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenTokenIsExpired()
    {
        // Arrange
        var expiredTokenValue =
            await CreateExpiredRefreshTokenAsync();

        var request = new RefreshTokenRequest(
            expiredTokenValue);

        // Act
        var response = await _client.PostAsJsonAsync(
            RefreshEndpoint,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    private async Task<TokenPairResponse> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync(
            LoginEndpoint,
            CreateLoginRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokenPair =
            await response.Content
                .ReadFromJsonAsync<TokenPairResponse>();

        Assert.NotNull(tokenPair);

        return tokenPair;
    }

    private static LoginRequest CreateLoginRequest()
    {
        return new LoginRequest(
            DemoEmail,
            DemoPassword);
    }

    private async Task<string> CreateExpiredRefreshTokenAsync()
    {
        /*
         * Creamos un scope porque AppDbContext es Scoped.
         * No debemos resolverlo directamente desde
         * _factory.Services sin crear primero el scope.
         */
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var refreshTokenService =
            scope.ServiceProvider
                .GetRequiredService<IRefreshTokenService>();

        var tokenValue =
            refreshTokenService.GenerateToken();

        var tokenHash =
            refreshTokenService.ComputeHash(tokenValue);

        var expiredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),

            /*
             * Debe coincidir con el UserId configurado
             * para el usuario de pruebas.
             */
            UserId = "demo-user",

            TokenHash = tokenHash,

            CreatedAtUtc =
                DateTime.UtcNow.AddDays(-10),

            ExpiresAtUtc =
                DateTime.UtcNow.AddDays(-1)
        };

        await dbContext.RefreshTokens.AddAsync(
            expiredToken);

        await dbContext.SaveChangesAsync();

        return tokenValue;
    }

    private sealed record LoginRequest(
        string Email,
        string Password);

    private sealed record RefreshTokenRequest(
        string RefreshToken);

    private sealed record RevokeRefreshTokenRequest(
        string RefreshToken);

    private sealed record TokenPairResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);
}