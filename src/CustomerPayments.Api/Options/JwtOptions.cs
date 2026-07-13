using System.ComponentModel.DataAnnotations;

namespace CustomerPayments.Api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string Audience { get; init; }

    [Required]
    [MinLength(32)]
    public required string SecretKey { get; init; }

    [Range(1, 1440)]
    public int ExpirationMinutes { get; init; }
}