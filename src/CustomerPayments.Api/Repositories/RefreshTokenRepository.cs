using CustomerPayments.Api.Data;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CustomerPayments.Api.Repositories;

public sealed class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;

    public RefreshTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}