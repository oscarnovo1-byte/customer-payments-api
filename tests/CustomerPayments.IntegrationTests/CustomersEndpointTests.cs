using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerPayments.Api.DTOs;
using FluentAssertions;

namespace CustomerPayments.IntegrationTests;

public class CustomersEndpointTests : IClassFixture<CustomerPaymentsApiFactory>
{
    private readonly HttpClient _client;

    private readonly CustomerPaymentsApiFactory _factory;

    public CustomersEndpointTests(CustomerPaymentsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.ResetDatabase();
    }

    [Fact]
    public async Task PostCustomer_ShouldReturnCreated()
    {
        await AuthenticateAsync();
        
        var unique = Guid.NewGuid().ToString("N");
        var email = $"john.doe.{unique}@test.com";

        var request = new CreateCustomerRequest(
            "John",
            "Doe",
            email,
            "123456789",
            "DNI123");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();

        customer.Should().NotBeNull();
        customer!.Id.Should().NotBeEmpty();
        customer.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.Email.Should().Be(email);
        customer.PhoneNumber.Should().Be("123456789");
        customer.DocumentNumber.Should().Be("DNI123");
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnOk()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/v1/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostCustomer_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        await AuthenticateAsync();

        var request = new CreateCustomerRequest(
            "John",
            "Doe",
            "invalid-email",
            null,
            null);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/v1/customers");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnPagedResponse()
    {
        await AuthenticateAsync();

        await CreateCustomerAsync("John", "Doe", "john.doe@test.com");
        await CreateCustomerAsync("Jane", "Smith", "jane.smith@test.com");
        await CreateCustomerAsync("Bob", "Brown", "bob.brown@test.com");

        var response = await _client.GetAsync(
            "/api/v1/customers?pageNumber=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerDto>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(3);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetCustomers_ShouldFilterBySearch()
    {
        await AuthenticateAsync();

        var unique = Guid.NewGuid().ToString("N");

        await CreateCustomerAsync("Alice", "Wonder", $"alice.{unique}@test.com");
        await CreateCustomerAsync("Carlos", "Rivera", $"carlos.{unique}@test.com");

        var response = await _client.GetAsync(
            $"/api/v1/customers?pageNumber=1&pageSize=10&search=alice.{unique}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerDto>>();

        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].Email.Should().Be($"alice.{unique}@test.com");
    }

    [Fact]
    public async Task GetCustomers_ShouldSortByLastNameAscending()
    {
        await AuthenticateAsync();

        var unique = Guid.NewGuid().ToString("N");

        await CreateCustomerAsync("John", "Zulu", $"john.zulu.{unique}@test.com");
        await CreateCustomerAsync("Jane", "Alpha", $"jane.alpha.{unique}@test.com");

        var response = await _client.GetAsync(
            $"/api/v1/customers?pageNumber=1&pageSize=10&search={unique}&sortBy=lastName&sortDirection=asc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerDto>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items[0].LastName.Should().Be("Alpha");
        result.Items[1].LastName.Should().Be("Zulu");
    }

    [Fact]
    public async Task GetCustomers_ShouldSortByLastNameDescending()
    {
        await AuthenticateAsync();

        var unique = Guid.NewGuid().ToString("N");

        await CreateCustomerAsync("John", "Zulu", $"john.zulu.{unique}@test.com");
        await CreateCustomerAsync("Jane", "Alpha", $"jane.alpha.{unique}@test.com");

        var response = await _client.GetAsync(
            $"/api/v1/customers?pageNumber=1&pageSize=10&search={unique}&sortBy=lastName&sortDirection=desc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerDto>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items[0].LastName.Should().Be("Zulu");
        result.Items[1].LastName.Should().Be("Alpha");
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnOk_WhenCustomerExists()
    {
        await AuthenticateAsync();

        var unique = Guid.NewGuid().ToString("N");
        var email = $"john.byid.{unique}@test.com";

        var createdCustomer = await CreateCustomerAsync(
            "John",
            "ById",
            email);

        var response = await _client.GetAsync(
            $"/api/v1/customers/{createdCustomer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();

        customer.Should().NotBeNull();
        customer!.Id.Should().Be(createdCustomer.Id);
        customer.Email.Should().Be(email);
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

    [Fact]
    public async Task GetCustomerById_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync(
            $"/api/v1/customers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<CustomerDto> CreateCustomerAsync(
        string firstName,
        string lastName,
        string email)
    {
        var request = new CreateCustomerRequest(
            firstName,
            lastName,
            email,
            null,
            null);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers",
            request);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"customer creation failed. Response body: {body}");

        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }
}