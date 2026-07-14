using CustomerPayments.Api.Interfaces.Caching;
using CustomerPayments.Infrastructure.Caching;
using Microsoft.AspNetCore.OutputCaching;

namespace CustomerPayments.Api.Services;
public sealed class OutputCacheService : ICacheService
{
    private readonly IOutputCacheStore _outputCacheStore;

    public OutputCacheService(
        IOutputCacheStore outputCacheStore)
    {
        _outputCacheStore = outputCacheStore;
    }

    public async Task InvalidateCustomersAsync(
        CancellationToken cancellationToken)
    {
        await _outputCacheStore.EvictByTagAsync(
            CacheTags.CustomersList,
            cancellationToken);

        await _outputCacheStore.EvictByTagAsync(
            CacheTags.CustomerById,
            cancellationToken);
    }
}