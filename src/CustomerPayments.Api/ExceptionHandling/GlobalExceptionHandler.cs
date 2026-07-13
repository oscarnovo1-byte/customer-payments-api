using System.ComponentModel.DataAnnotations;
using CustomerPayments.Api.Constants;
using CustomerPayments.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CustomerPayments.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var correlationId = GetCorrelationId(httpContext);
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred while processing the request. TraceId: {TraceId}. CorrelationId: {CorrelationId}",
                httpContext.TraceIdentifier,
                correlationId);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "A handled business exception occurred while processing the request. TraceId: {TraceId}. CorrelationId: {CorrelationId}",
                httpContext.TraceIdentifier,
                correlationId);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = exception.Message,
            Type = GetTypeUri(statusCode),
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions[CorrelationIdConstants.PropertyName] = correlationId;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status404NotFound => "Resource not found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Internal server error"
    };

    private static string GetTypeUri(int statusCode) => statusCode switch
    {
        StatusCodes.Status404NotFound => "https://httpstatuses.com/404",
        StatusCodes.Status409Conflict => "https://httpstatuses.com/409",
        _ => "https://httpstatuses.com/500"
    };

    private static string? GetCorrelationId(HttpContext context)
    {
        return context.Items.TryGetValue(
            CorrelationIdConstants.PropertyName,
            out var correlationId)
            ? correlationId?.ToString()
            : null;
    }
}
