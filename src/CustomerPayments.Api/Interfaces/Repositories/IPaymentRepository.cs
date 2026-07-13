using CustomerPayments.Api.Domain.Entities;

namespace CustomerPayments.Api.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task AddAsync(Payment payment, CancellationToken cancellationToken);
}