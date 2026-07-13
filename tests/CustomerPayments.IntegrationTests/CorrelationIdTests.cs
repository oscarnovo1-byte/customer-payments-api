using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CustomerPayments.IntegrationTests;

public sealed class CorrelationIdTests : IClassFixture<CustomerPaymentsApiFactory>
{
    private readonly HttpClient _client;

    public CorrelationIdTests(CustomerPaymentsApiFactory factory)
    {
        _client = factory.CreateClient();
        factory.ResetDatabase();
    }

    [Fact]
    public async Task Health_ShouldReturnCorrelationId_WhenHeaderIsProvided()
    {
        const string correlationId = "test-correlation-id";

        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", correlationId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Correlation-Id")
            .Should()
            .Contain(correlationId);
    }

    [Fact]
    public async Task CustomerById_ShouldReturnProblemDetailsWithCorrelationId_WhenCustomerDoesNotExist()
    {
        await AuthenticateAsync();

        const string correlationId = "customer-not-found-test";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/customers/{Guid.NewGuid()}");

        request.Headers.Add("X-Correlation-Id", correlationId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("correlationId");
        problemDetails.Extensions["correlationId"]!.ToString()
            .Should()
            .Be(correlationId);
    }

    private async Task AuthenticateAsync()
    {
        var loginRequest = new CustomerPayments.Api.DTOs.LoginRequest(
            "admin@customerpayments.com",
            "Admin123!");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            loginRequest);

        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content
            .ReadFromJsonAsync<CustomerPayments.Api.DTOs.LoginResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                loginResponse!.AccessToken);
    }
}