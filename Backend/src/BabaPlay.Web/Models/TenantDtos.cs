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

    [Required(ErrorMessage = "A rua é obrigatória.")]
    [StringLength(160, ErrorMessage = "A rua deve ter no máximo 160 caracteres.")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "O número é obrigatório.")]
    [StringLength(30, ErrorMessage = "O número deve ter no máximo 30 caracteres.")]
    public string Number { get; set; } = string.Empty;

    [StringLength(120, ErrorMessage = "O bairro deve ter no máximo 120 caracteres.")]
    public string? Neighborhood { get; set; }

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CEP é obrigatório.")]
    [StringLength(20, ErrorMessage = "O CEP deve ter no máximo 20 caracteres.")]
    public string ZipCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "A latitude é obrigatória.")]
    [Range(-90, 90, ErrorMessage = "A latitude deve estar entre -90 e 90.")]
    public double? AssociationLatitude { get; set; }

    [Required(ErrorMessage = "A longitude é obrigatória.")]
    [Range(-180, 180, ErrorMessage = "A longitude deve estar entre -180 e 180.")]
    public double? AssociationLongitude { get; set; }
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

public record TenantSettingsDto(
    Guid Id,
    string Name,
    string Slug,
    int PlayersPerTeam,
    string? LogoPath,
    string? Street,
    string? Number,
    string? Neighborhood,
    string? City,
    string? State,
    string? ZipCode,
    double? AssociationLatitude,
    double? AssociationLongitude);

public record UpdateTenantSettingsDto
{
    [Required(ErrorMessage = "O nome da associação é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Range(2, 22, ErrorMessage = "O número de jogadores por time deve ser entre 2 e 22.")]
    public int PlayersPerTeam { get; set; } = 10;

    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public double? AssociationLatitude { get; set; }
    public double? AssociationLongitude { get; set; }
}

public record TenantGameDayOptionDto(
    Guid Id,
    Guid TenantId,
    DayOfWeek DayOfWeek,
    TimeOnly LocalStartTime,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateTenantGameDayOptionDto
{
    [Required(ErrorMessage = "O dia da semana é obrigatório.")]
    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Saturday;

    [Required(ErrorMessage = "O horário de início é obrigatório.")]
    public TimeOnly LocalStartTime { get; set; } = new TimeOnly(8, 0);
}

