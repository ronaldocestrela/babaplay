using Application.Features.Associations.Commands;
using FluentValidation;

namespace Application.Features.Associations.Validations;

public class CreateAssociationCommandValidator : AbstractValidator<CreateAssociationCommand>
{
    public CreateAssociationCommandValidator()
    {
        RuleFor(command => command.CreateAssociation)
            .SetValidator(new CreateAssociationRequestValidator());
    }
}
