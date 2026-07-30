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

public record PositionDto(
    Guid Id,
    string Name,
    string Abbreviation);
