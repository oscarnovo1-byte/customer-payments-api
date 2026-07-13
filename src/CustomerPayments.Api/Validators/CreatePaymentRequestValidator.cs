using CustomerPayments.Api.DTOs;
using FluentValidation;
namespace CustomerPayments.Api.Validators;
public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.PaymentDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Method)
            .IsInEnum();

        RuleFor(x => x.Description)
            .MaximumLength(300);

        RuleFor(x => x.ExternalReference)
            .MaximumLength(100);
    }
}