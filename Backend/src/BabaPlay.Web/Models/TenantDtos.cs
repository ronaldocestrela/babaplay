using System;
using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Web.Models;

public record TenantSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string Role);

public record CreateTenantDto
{
    [Required(ErrorMessage = "O nome da associação é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O slug (identificador único na URL) é obrigatório.")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "O slug deve conter apenas letras minúsculas, números e hífens.")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail do administrador é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha do administrador é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    public string AdminPassword { get; set; } = string.Empty;

    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Neighborhood { get; set; }

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    public string State { get; set; } = string.Empty;

    public string? ZipCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public record AcceptInviteDto
{
    [Required(ErrorMessage = "O token de convite é obrigatório.")]
    public string Token { get; set; } = string.Empty;
}

public record InviteValidationDto(
    bool IsValid,
    string TenantName,
    string Email,
    string Role,
    string? ErrorMessage);
