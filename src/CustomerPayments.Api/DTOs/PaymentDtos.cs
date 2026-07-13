using CustomerPayments.Api.Domain.Enums;

namespace CustomerPayments.Api.DTOs;
public sealed record PaymentDto(
    Guid Id,
    Guid CustomerId,
    DateOnly PaymentDate,
    decimal Amount,
    PaymentStatus Status,
    PaymentMethod Method,
    string? Description,
    string? ExternalReference,
    string? CreatedBy,
    DateTime CreatedAtUtc
);

public sealed record CreatePaymentRequest(
    DateOnly PaymentDate,
    decimal Amount,
    PaymentMethod Method,
    string? Description,
    string? ExternalReference
);