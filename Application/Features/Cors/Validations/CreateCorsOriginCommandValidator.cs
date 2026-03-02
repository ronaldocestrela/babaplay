using Application.Features.Cors.Commands;
using FluentValidation;

namespace Application.Features.Cors.Validations;

public class CreateCorsOriginCommandValidator : AbstractValidator<CreateCorsOriginCommand>
{
    public CreateCorsOriginCommandValidator()
    {
        RuleFor(cmd => cmd.CreateCors)
            .SetValidator(new CreateCorsOriginRequestValidator());
    }
}