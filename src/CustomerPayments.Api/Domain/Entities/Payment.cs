using CustomerPayments.Api.Domain.Enums;

namespace CustomerPayments.Api.Domain.Entities;

public sealed class Payment : BaseEntity
{
    private Payment()
    {
    }

    public Guid CustomerId { get; private init; }

    public DateOnly PaymentDate { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public PaymentMethod Method { get; private set; }

    public string? Description { get; private set; }

    public string? ExternalReference { get; private set; }

    public Customer Customer { get; private init; } = null!;

    public static Payment Create(
        Guid customerId,
        DateOnly paymentDate,
        decimal amount,
        PaymentMethod method,
        string? description,
        string? externalReference,
        string? createdBy)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PaymentDate = paymentDate,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Paid,
            Description = description,
            ExternalReference = externalReference,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void MarkAsPaid()
    {
        Status = PaymentStatus.Paid;
    }

    public void MarkAsFailed()
    {
        Status = PaymentStatus.Failed;
    }

    public void Refund()
    {
        Status = PaymentStatus.Refunded;
    }
}