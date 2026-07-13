using CustomerPayments.Api.DTOs;

namespace CustomerPayments.Api.Interfaces.Services;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentDto>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task<PaymentDto> CreateAsync(
        Guid customerId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken);
}