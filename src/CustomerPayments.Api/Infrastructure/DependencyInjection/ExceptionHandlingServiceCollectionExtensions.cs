using CustomerPayments.Api.ExceptionHandling;

public static class ExceptionHandlingServiceCollectionExtensions
{
    public static IServiceCollection AddExceptionHandling(
        this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}