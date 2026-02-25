using BabaPlayShared.Library.Models.Requests.Associations;
using FluentValidation;

namespace Application.Features.Associations.Validations;

internal class CreateAssociationRequestValidator : AbstractValidator<CreateAssociationRequest>
{
    public CreateAssociationRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Association name is required.")
            .MaximumLength(100).WithMessage("Association name must not exceed 100 characters.");

        RuleFor(request => request.EstablishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Established date cannot be in the future.");
    }
}
