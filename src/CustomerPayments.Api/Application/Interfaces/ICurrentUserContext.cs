namespace CustomerPayments.Api.Application.Interfaces;

public interface ICurrentUserContext
{
    string? UserId { get; }

    string? Email { get; }

    string? Role { get; }

    bool IsAuthenticated { get; }
}