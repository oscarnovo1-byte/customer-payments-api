using CustomerPayments.Api.Infrastructure.DependencyInjection;
using CustomerPayments.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
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

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Payments API v1");
    });
}

app.UseHttpsRedirection();

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