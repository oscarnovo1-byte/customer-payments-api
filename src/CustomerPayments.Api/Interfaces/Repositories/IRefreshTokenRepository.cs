using CustomerPayments.Api.Domain.Entities;

namespace CustomerPayments.Api.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}