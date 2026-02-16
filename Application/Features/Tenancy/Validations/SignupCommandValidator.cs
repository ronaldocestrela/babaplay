using FluentValidation;
using Application.Features.Tenancy.Commands;

namespace Application.Features.Tenancy.Validations;

public class SignupCommandValidator : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(c => c.SignupRequest)
            .SetValidator(new SignupRequestValidator());
    }
}
