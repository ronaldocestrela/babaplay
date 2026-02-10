using FluentValidation;

namespace Application.Features.Associados.Validations;

internal class CreateAssociadoRequestValidator : AbstractValidator<CreateAssociadoRequest>
{
    public CreateAssociadoRequestValidator()
    {
        RuleFor(r => r.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("Nome completo não pode exceder 200 caracteres.");

        RuleFor(r => r.CPF)
            .NotEmpty().WithMessage("CPF é obrigatório.")
            .Length(11).WithMessage("CPF deve conter 11 dígitos.")
            .Matches(@"^\d{11}$").WithMessage("CPF deve conter apenas números.");

        RuleFor(r => r.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Data de nascimento deve ser no passado.");

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

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres.");

        RuleFor(r => r.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória.")
            .Equal(r => r.Password).WithMessage("Senhas não conferem.");
    }
}
