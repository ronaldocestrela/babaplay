using Domain.Entities;
using FluentValidation;

namespace Application.Features.Associations.Validations;

public class UpdateAssociationRequestValidator : AbstractValidator<UpdateAssociationRequest>
{
    public UpdateAssociationRequestValidator(IAssociationService associationService)
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Association ID is required.")
            .MustAsync(async (id, cancellation) => await associationService.GetByIdAsync(id) is Association associationInDb && associationInDb.Id == id)
            .WithMessage("Association with the specified ID does not exist.");

        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Association name is required.")
            .MaximumLength(100).WithMessage("Association name must not exceed 100 characters.");

        RuleFor(request => request.EstablishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Established date cannot be in the future.");
    }
}
