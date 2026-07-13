using CustomerPayments.Api.Data;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CustomerPayments.Api.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}