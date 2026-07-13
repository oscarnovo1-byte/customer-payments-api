using System.Net;
using System.Net.Http.Json;
using CustomerPayments.Api.DTOs;
using FluentAssertions;

namespace CustomerPayments.IntegrationTests;

public sealed class RateLimitingTests : IClassFixture<CustomerPaymentsApiFactory>
{
    private readonly HttpClient _client;

    public RateLimitingTests(CustomerPaymentsApiFactory factory)
    {
        _client = factory.CreateClient();
        factory.ResetDatabase();
    }

    [Fact]
    public async Task Login_ShouldReturnTooManyRequests_WhenRateLimitIsExceeded()
    {
        for (var i = 0; i < 5; i++)
        {
            var request = new LoginRequest(
                "admin@customerpayments.com",
                "WrongPassword");

            var response = await _client.PostAsJsonAsync(
                "/api/v1/auth/login",
                request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var lastRequest = new LoginRequest(
            "admin@customerpayments.com",
            "WrongPassword");

        var lastResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            lastRequest);

        lastResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}