using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace CustomerPayments.Api.Infrastructure.DependencyInjection;

public static class RateLimiterServiceCollectionExtensions
{
    public static IServiceCollection AddApiRateLimiter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("FixedWindowPolicy", limiterOptions =>
            {
                limiterOptions.PermitLimit =
                    configuration.GetValue<int>("RateLimiting:ApiPermitLimit", 20);

                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
            {
                limiterOptions.PermitLimit =
                    configuration.GetValue<int>("RateLimiting:AuthPermitLimit", 5);

                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });

        return services;
    }
}