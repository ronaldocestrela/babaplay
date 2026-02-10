using FluentValidation;

namespace Application.Features.Associados.Validations;

internal class UpdateAssociadoRequestValidator : AbstractValidator<UpdateAssociadoRequest>
{
    public UpdateAssociadoRequestValidator()
    {
        RuleFor(r => r.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("Nome completo não pode exceder 200 caracteres.");

        RuleFor(r => r.PhoneNumber)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .MaximumLength(20).WithMessage("Telefone não pode exceder 20 caracteres.");

        RuleFor(r => r.Address)
            .NotEmpty().WithMessage("Endereço é obrigatório.")
            .MaximumLength(300).WithMessage("Endereço não pode exceder 300 caracteres.");

        RuleFor(r => r.City)
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres.");

        RuleFor(r => r.State)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .Length(2).WithMessage("Estado deve conter 2 caracteres (sigla).");

        RuleFor(r => r.ZipCode)
            .NotEmpty().WithMessage("CEP é obrigatório.")
            .MaximumLength(10).WithMessage("CEP não pode exceder 10 caracteres.");

        RuleFor(r => r.Position)
            .NotEmpty().WithMessage("Posição é obrigatória.")
            .MaximumLength(50).WithMessage("Posição não pode exceder 50 caracteres.");
    }
}
