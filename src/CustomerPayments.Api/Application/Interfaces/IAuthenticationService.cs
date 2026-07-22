using CustomerPayments.Api.DTOs;

namespace CustomerPayments.Api.Application.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);

    Task<LoginResponse?> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken);

    Task<bool> RevokeAsync(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken);
}