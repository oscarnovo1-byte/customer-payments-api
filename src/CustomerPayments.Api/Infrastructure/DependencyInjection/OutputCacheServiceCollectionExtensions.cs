namespace CustomerPayments.Api.Infrastructure.DependencyInjection;

public static class OutputCacheServiceCollectionExtensions
{
    public static IServiceCollection AddApiOutputCache(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.AddPolicy("CustomersReadPolicy", policy =>
            {
                policy
                    .Tag("customers-list")
                    .Expire(TimeSpan.FromSeconds(60))
                    .SetVaryByQuery(
                        "pageNumber",
                        "pageSize",
                        "search",
                        "sortBy",
                        "sortDirection");
            });

            options.AddPolicy("CustomerByIdReadPolicy", policy =>
            {
                policy
                    .Tag("customers-by-id")
                    .Expire(TimeSpan.FromSeconds(60));
            });

            options.AddPolicy("PaymentsReadPolicy", policy =>
            {
                policy
                    .Tag("payments")
                    .Expire(TimeSpan.FromSeconds(60));
            });
        });

        return services;
    }
}