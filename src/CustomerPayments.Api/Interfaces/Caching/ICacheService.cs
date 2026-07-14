namespace CustomerPayments.Api.Interfaces.Caching;
public interface ICacheService
{
    Task InvalidateCustomersAsync(
        CancellationToken cancellationToken);
}