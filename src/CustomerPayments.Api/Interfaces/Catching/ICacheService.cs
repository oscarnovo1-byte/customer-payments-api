namespace CustomerPayments.Api.Interfaces.Catching;
public interface ICacheService
{
    Task InvalidateCustomersAsync(
        CancellationToken cancellationToken);
}