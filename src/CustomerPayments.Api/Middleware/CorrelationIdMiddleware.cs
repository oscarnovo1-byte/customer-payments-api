using CustomerPayments.Api.Constants;
using Serilog.Context;

namespace CustomerPayments.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.TraceIdentifier = correlationId;

        context.Items[CorrelationIdConstants.PropertyName] = correlationId;

        context.Response.Headers[
            CorrelationIdConstants.HeaderName] = correlationId;

        using (LogContext.PushProperty(
            CorrelationIdConstants.PropertyName,
            correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(
                CorrelationIdConstants.HeaderName,
                out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}