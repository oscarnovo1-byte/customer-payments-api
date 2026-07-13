using CustomerPayments.Api.Application.Models;

namespace CustomerPayments.Api.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(UserTokenData user, DateTime expiresAtUtc);
}