using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Data;
using CustomerPayments.Api.Infrastructure.Context;
using CustomerPayments.Api.Interfaces.Repositories;
using CustomerPayments.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CustomerPayments.Api.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentRequestContext, CurrentRequestContext>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        return services;
    }
}