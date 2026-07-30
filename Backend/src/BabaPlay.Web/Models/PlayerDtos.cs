using System;
using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Web.Models;

public record CompleteProfileDto
{
    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O apelido é obrigatório.")]
    public string Nickname { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Selecione seu pé preferido.")]
    public string PreferredFoot { get; set; } = "Right"; // Right, Left, Both

    [Required(ErrorMessage = "Selecione sua posição principal.")]
    public Guid PrimaryPositionId { get; set; }

    [Range(1, 99, ErrorMessage = "O número da camisa deve estar entre 1 e 99.")]
    public int? JerseyNumber { get; set; }
}

public record PlayerProfileResponseDto(
    Guid Id,
    string Name,
    string Nickname,
    string PreferredFoot,
    string PositionName,
    int? JerseyNumber,
    string? PhotoUrl);

public record PlayerDto(
    Guid Id,
    Guid UserId,
    string Name,
    string? Nickname,
    string? Phone,
    DateTime? DateOfBirth,
    string? PreferredFoot,
    Guid? PrimaryPositionId,
    string? PrimaryPositionName,
    int? JerseyNumber,
    string? PhotoUrl,
    Guid? RoleId,
    string? RoleName,
    bool IsActive,
    DateTime CreatedAt);

public record RoleDto(
    Guid Id,
    string Name,
    string? Description);

public record UpdatePlayerAdminDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Name { get; set; } = string.Empty;

    public string? Nickname { get; set; }

    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string PreferredFoot { get; set; } = "Right";

    public Guid? PrimaryPositionId { get; set; }

    [Range(1, 99, ErrorMessage = "O número da camisa deve ser entre 1 e 99.")]
    public int? JerseyNumber { get; set; }

    public Guid? RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}

public record PositionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description = null,
    bool IsActive = true)
{
    public PositionDto(Guid id, string name, string abbreviation)
        : this(id, abbreviation, name, null, true) { }

    public string Abbreviation => Code;
}

public record CreatePositionDto
{
    [Required(ErrorMessage = "A sigla/código é obrigatória.")]
    [StringLength(10, ErrorMessage = "A sigla deve ter no máximo 10 caracteres.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome da posição é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome deve ter no máximo 50 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public record UpdatePositionDto
{
    [Required(ErrorMessage = "A sigla/código é obrigatória.")]
    [StringLength(10, ErrorMessage = "A sigla deve ter no máximo 10 caracteres.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome da posição é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome deve ter no máximo 50 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}


