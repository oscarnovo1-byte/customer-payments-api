using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Constants;

namespace CustomerPayments.Api.Infrastructure.Context;

public sealed class CurrentRequestContext : ICurrentRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentRequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CorrelationId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            return httpContext.Items.TryGetValue(
                CorrelationIdConstants.PropertyName,
                out var correlationId)
                ? correlationId?.ToString()
                : null;
        }
    }
}