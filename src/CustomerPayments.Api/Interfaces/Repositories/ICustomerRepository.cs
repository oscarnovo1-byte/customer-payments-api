using CustomerPayments.Api.Domain.Entities;

namespace CustomerPayments.Api.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Customer?> GetTrackedByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Customer customer,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Customer>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? sortBy,
        bool sortDescending,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken);
}