using CustomerPayments.Api.Constants;
using CustomerPayments.Api.Infrastructure.DependencyInjection;
using CustomerPayments.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddApplicationOptions(builder.Configuration);
builder.Services.AddExceptionHandling();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiDocumentation();
builder.Services.AddApiOutputCache();
builder.Services.AddApiRateLimiter(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.EnrichDiagnosticContext = (
        diagnosticContext,
        httpContext) =>
    {
        diagnosticContext.Set(
            "RequestHost",
            httpContext.Request.Host.Value);

        diagnosticContext.Set(
            "RequestScheme",
            httpContext.Request.Scheme);

        diagnosticContext.Set(
            "UserAgent",
            httpContext.Request.Headers.UserAgent.ToString());

        diagnosticContext.Set(
            CorrelationIdConstants.PropertyName,
            httpContext.TraceIdentifier);
    };
});

app.UseExceptionHandler();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Payments API v1");
    });
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapHealthChecks("/health");

app.MapControllers();

await app.ApplyDatabaseMigrationsAsync();

app.Run();

public partial class Program
{
}