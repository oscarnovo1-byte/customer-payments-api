using CustomerPayments.Api.Exceptions;

namespace CustomerPayments.Api.Domain.Entities;

public sealed class Customer : BaseEntity
{
    private Customer()
    {
    }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public string? PhoneNumber { get; private set; }

    public string? DocumentNumber { get; private set; }

    public bool IsActive { get; private set; }

    public ICollection<Payment> Payments { get; private set; } = [];

    public static Customer Create(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string? documentNumber,
        string? createdBy)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            DocumentNumber = documentNumber,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void UpdateContactInfo(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string? documentNumber,
        int expectedVersion,
        string? updatedBy)
    {
        if (Version != expectedVersion)
        {
            throw new ConflictException("The customer was modified by another process.");
        }

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        DocumentNumber = documentNumber;
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void Deactivate(
        int expectedVersion,
        string? updatedBy)
    {
        if (Version != expectedVersion)
        {
            throw new ConflictException("The customer was modified by another process.");
        }

        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}