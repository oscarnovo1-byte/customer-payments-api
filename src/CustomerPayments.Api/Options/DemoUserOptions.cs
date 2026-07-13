using System.ComponentModel.DataAnnotations;

namespace CustomerPayments.Api.Options;

public sealed class DemoUserOptions
{
    public const string SectionName = "DemoUser";

    [Required]
    public required string UserId { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }

    [Required]
    public required string Role { get; init; }
}