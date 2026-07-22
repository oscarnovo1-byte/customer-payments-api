using CustomerPayments.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CustomerPayments.Api.Infrastructure.DependencyInjection;

public static class DatabaseInitializationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(
        this WebApplication app)
    {
        var shouldApplyMigrations =
            app.Environment.IsDevelopment()
            || app.Environment.IsEnvironment("Docker");

        if (!shouldApplyMigrations)
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}