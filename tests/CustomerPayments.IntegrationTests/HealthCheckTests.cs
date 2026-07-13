using System.Net;
using FluentAssertions;

namespace CustomerPayments.IntegrationTests;

public class HealthCheckTests : IClassFixture<CustomerPaymentsApiFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(CustomerPaymentsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
