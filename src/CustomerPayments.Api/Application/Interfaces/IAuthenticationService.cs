using CustomerPayments.Api.DTOs;

namespace CustomerPayments.Api.Application.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}