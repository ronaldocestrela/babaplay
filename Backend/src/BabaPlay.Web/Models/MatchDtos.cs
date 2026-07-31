using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BabaPlay.Web.Serialization;

namespace BabaPlay.Web.Models;

public record MatchDto(
    Guid Id,
    Guid GameDayId,
    string? GameDayName,
    DateTime ScheduledAt,
    string? Location,
    string? Description,
    string? HomeTeamName,
    string? AwayTeamName,
    [property: JsonConverter(typeof(MatchStatusStringConverter))]
    string Status, // "Pending", "Scheduled", "InProgress", "Completed", "Cancelled"
    int MaxPlayers,
    int ConfirmedCount);

public record CreateMatchDto
{
    [Required(ErrorMessage = "Selecione o dia do baba.")]
    public Guid GameDayId { get; set; }

    public Guid? HomeTeamId { get; set; }

    public Guid? AwayTeamId { get; set; }

    [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
    public string? Description { get; set; }
}

public record UpdateMatchDto
{
    [Required(ErrorMessage = "Selecione o dia do baba.")]
    public Guid GameDayId { get; set; }

    public Guid? HomeTeamId { get; set; }

    public Guid? AwayTeamId { get; set; }

    [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
    public string? Description { get; set; }
}

public record ScheduleGameDayDto
{
    [Required(ErrorMessage = "O nome do evento é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data e hora do evento são obrigatórias.")]
    public DateTime ScheduledAt { get; set; } = DateTime.Now.AddDays(1);

    [Required(ErrorMessage = "O local da partida é obrigatório.")]
    public string Location { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(1, 100, ErrorMessage = "O número máximo de jogadores deve ser de pelo menos 1.")]
    public int MaxPlayers { get; set; } = 20;

    /// <summary>Initial status: "Pending" (rascunho) or "Confirmed" (agendado). Default Confirmed.</summary>
    [Required(ErrorMessage = "Selecione o status do agendamento.")]
    public string Status { get; set; } = "Confirmed";
}

public record GameDayDto(
    Guid Id,
    string Name,
    DateTime ScheduledAt,
    string? Location,
    string? Description,
    int MaxPlayers,
    [property: JsonConverter(typeof(GameDayStatusStringConverter))]
    string Status);

public record ChangeMatchStatusDto(string Status);

public record ChangeGameDayStatusDto(int Status);
