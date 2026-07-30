using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Web.Models;

public record LoginDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserProfileDto User);

public record ForgotPasswordDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    public string ResetLinkBaseUrl { get; set; } = string.Empty;
}

public record ResetPasswordDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O token de recuperação é obrigatório.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
    [Compare(nameof(Password), ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public record UserProfileDto(
    string Id,
    string Email,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime CreatedAt,
    TenantSummaryDto? DefaultTenant,
    IReadOnlyList<TenantSummaryDto> Memberships);
