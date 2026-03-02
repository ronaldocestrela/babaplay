using Application.Features.Cors.Commands;
using FluentValidation;

namespace Application.Features.Cors.Validations;

public class UpdateCorsOriginCommandValidator : AbstractValidator<UpdateCorsOriginCommand>
{
    public UpdateCorsOriginCommandValidator()
    {
        RuleFor(cmd => cmd.UpdateCors)
            .SetValidator(new UpdateCorsOriginRequestValidator());
    }
}