using CustomerPayments.Api.DTOs;

namespace CustomerPayments.Api.Interfaces.Services;

public interface ICustomerService
{
    Task<PagedResponse<CustomerDto>> GetAllAsync(
    PagedRequest request,
    CancellationToken cancellationToken);

    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CustomerDto> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken);

    Task<CustomerDto> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken);

    Task DeactivateAsync(
        Guid id,
        DeactivateCustomerRequest request,
        CancellationToken cancellationToken);
}