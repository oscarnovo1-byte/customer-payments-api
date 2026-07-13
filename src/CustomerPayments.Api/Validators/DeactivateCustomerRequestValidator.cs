using CustomerPayments.Api.DTOs;
using FluentValidation;

namespace CustomerPayments.Api.Validators;

public sealed class DeactivateCustomerRequestValidator : AbstractValidator<DeactivateCustomerRequest>
{
    public DeactivateCustomerRequestValidator()
    {
        RuleFor(x => x.Version)
            .GreaterThan(0);
    }
}