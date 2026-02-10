using Application.Features.Associados.Commands;
using FluentValidation;

namespace Application.Features.Associados.Validations;

public class UpdateAssociadoCommandValidator : AbstractValidator<UpdateAssociadoCommand>
{
    public UpdateAssociadoCommandValidator()
    {
        RuleFor(command => command.AssociadoId)
            .NotEmpty().WithMessage("ID do associado é obrigatório.");

        RuleFor(command => command.UpdateAssociado)
            .SetValidator(new UpdateAssociadoRequestValidator());
    }
}
