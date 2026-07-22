namespace CustomerPayments.Api.Interfaces.Services;
public interface IRefreshTokenService
{
    string GenerateToken();

    string ComputeHash(string token);
}