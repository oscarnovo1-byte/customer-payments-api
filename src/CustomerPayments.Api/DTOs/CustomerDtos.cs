namespace CustomerPayments.Api.DTOs;

public sealed record CustomerDto
{
    public Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? DocumentNumber { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
    public int Version { get; init; }
}

public sealed record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? DocumentNumber
);

public sealed record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? DocumentNumber,
    int Version);