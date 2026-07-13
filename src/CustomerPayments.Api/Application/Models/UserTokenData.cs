namespace CustomerPayments.Api.Application.Models;

public sealed record UserTokenData(
    string UserId,
    string Email,
    string Role
);