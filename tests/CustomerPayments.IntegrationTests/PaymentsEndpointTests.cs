using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerPayments.Api.Domain.Enums;
using CustomerPayments.Api.DTOs;
using FluentAssertions;

namespace CustomerPayments.IntegrationTests;

public class PaymentsEndpointTests : IClassFixture<CustomerPaymentsApiFactory>
{
    private readonly HttpClient _client;

    public PaymentsEndpointTests(CustomerPaymentsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostPayment_ShouldReturnCreated_WhenCustomerExists()
    {
        await AuthenticateAsync();

        var customer = await CreateCustomerAsync();

        var paymentRequest = new CreatePaymentRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            250,
            PaymentMethod.CreditCard,
            "Monthly payment",
            "EXT-001");

        var paymentResponse = await _client.PostAsJsonAsync(
            $"/api/v1/customers/{customer.Id}/payments",
            paymentRequest);

        paymentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var payment = await paymentResponse.Content.ReadFromJsonAsync<PaymentDto>();

        payment.Should().NotBeNull();
        payment!.Id.Should().NotBeEmpty();
        payment.CustomerId.Should().Be(customer.Id);
        payment.Amount.Should().Be(250);
        payment.Method.Should().Be(PaymentMethod.CreditCard);
        payment.Description.Should().Be("Monthly payment");
        payment.ExternalReference.Should().Be("EXT-001");
    }

    [Fact]
    public async Task GetPayments_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync(
            $"/api/v1/customers/{Guid.NewGuid()}/payments");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostPayment_ShouldReturnBadRequest_WhenAmountIsInvalid()
    {
        await AuthenticateAsync();

        var customer = await CreateCustomerAsync();

        var paymentRequest = new CreatePaymentRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            0,
            PaymentMethod.Cash,
            null,
            null);

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/customers/{customer.Id}/payments",
            paymentRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPayments_ShouldReturnOk_WhenCustomerExists()
    {
        await AuthenticateAsync();

        var customer = await CreateCustomerAsync();

        var response = await _client.GetAsync(
            $"/api/v1/customers/{customer.Id}/payments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payments = await response.Content.ReadFromJsonAsync<IReadOnlyList<PaymentDto>>();

        payments.Should().NotBeNull();
    }
    private async Task<CustomerDto> CreateCustomerAsync()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var customerRequest = new CreateCustomerRequest(
            "Jane",
            "Smith",
            $"jane.smith.{uniqueId}@test.com",
            null,
            null);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers",
            customerRequest);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"customer creation failed. Response body: {body}");

        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }

    private async Task AuthenticateAsync()
    {
        var loginRequest = new LoginRequest(
            "admin@customerpayments.com",
            "Admin123!");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            loginRequest);

        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
    }
}