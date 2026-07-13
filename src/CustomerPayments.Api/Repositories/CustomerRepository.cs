using CustomerPayments.Api.Data;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CustomerPayments.Api.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetTrackedByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Email == email, cancellationToken);
    }

    public async Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? sortBy,
        bool sortDescending,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var query = BuildSearchQuery(search, includeInactive);

        query = ApplySorting(query, sortBy, sortDescending);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var query = BuildSearchQuery(search, includeInactive);

        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<Customer> BuildSearchQuery(
        string? search,
        bool includeInactive)
    {
        var query = _context.Customers.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(customer => customer.IsActive);
        }

        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var normalizedSearch = search.Trim();

        return query.Where(customer =>
            customer.FirstName.Contains(normalizedSearch) ||
            customer.LastName.Contains(normalizedSearch) ||
            customer.Email.Contains(normalizedSearch));
    }

    private static IQueryable<Customer> ApplySorting(
    IQueryable<Customer> query,
    string? sortBy,
    bool sortDescending)
    {
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "firstname" => sortDescending
                ? query.OrderByDescending(customer => customer.FirstName)
                : query.OrderBy(customer => customer.FirstName),

            "lastname" => sortDescending
                ? query.OrderByDescending(customer => customer.LastName)
                : query.OrderBy(customer => customer.LastName),

            "email" => sortDescending
                ? query.OrderByDescending(customer => customer.Email)
                : query.OrderBy(customer => customer.Email),

            "createdatutc" => sortDescending
                ? query.OrderByDescending(customer => customer.CreatedAtUtc)
                : query.OrderBy(customer => customer.CreatedAtUtc),

            _ => query
                .OrderBy(customer => customer.LastName)
                .ThenBy(customer => customer.FirstName)
        };
    }
}