using Application.Features.Associations.Commands;
using FluentValidation;

namespace Application.Features.Associations.Validations;

public class UpdateAssociationCommandValidator : AbstractValidator<UpdateAssociationCommand>
{
    public UpdateAssociationCommandValidator(IAssociationService associationService)
    {
        RuleFor(command => command.UpdateAssociation)
            .SetValidator(new UpdateAssociationRequestValidator(associationService));
    }
}
