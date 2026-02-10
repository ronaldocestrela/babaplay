using Application.Features.Associados.Commands;
using FluentValidation;

namespace Application.Features.Associados.Validations;

public class CreateAssociadoCommandValidator : AbstractValidator<CreateAssociadoCommand>
{
    public CreateAssociadoCommandValidator()
    {
        RuleFor(command => command.CreateAssociado)
            .SetValidator(new CreateAssociadoRequestValidator());
    }
}
