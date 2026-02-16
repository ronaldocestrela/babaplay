using FluentValidation;
using Application.Features.Associados.Validations;

namespace Application.Features.Tenancy.Validations;

public class SignupRequestValidator : AbstractValidator<SignupRequest>
{
    public SignupRequestValidator()
    {
        RuleFor(r => r.AssociationName)
            .NotEmpty().WithMessage("Nome da associação é obrigatório.")
            .MaximumLength(200).WithMessage("Nome da associação não pode exceder 200 caracteres.");

        RuleFor(r => r.Address)
            .NotEmpty().WithMessage("Endereço da associação é obrigatório.")
            .MaximumLength(300);

        RuleFor(r => r.City).NotEmpty().MaximumLength(100);
        RuleFor(r => r.State).NotEmpty().Length(2);
        RuleFor(r => r.ZipCode).NotEmpty().MaximumLength(10);
        RuleFor(r => r.PhoneNumber).NotEmpty().MaximumLength(20);

        RuleFor(r => r.Admin).NotNull().WithMessage("Dados do administrador são obrigatórios.")
            .SetValidator(new CreateAssociadoRequestValidator());
    }
}
