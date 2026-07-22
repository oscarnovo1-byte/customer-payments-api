using System.Security.Cryptography;
using System.Text;
using CustomerPayments.Api.Interfaces.Services;


namespace CustomerPayments.Api.Services;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string ComputeHash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var hashBytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hashBytes);
    }
}