using CustomerPayments.Api.DTOs;
using FluentValidation;
namespace CustomerPayments.Api.Validators;
public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30);

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(50);
    }
}