using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Web.Models;

// ─── Response DTOs ────────────────────────────────────────────────────────────

public sealed record PollOptionDto(
    Guid Id,
    string Text,
    int Order,
    int VotesCount,
    double Percentage);

public sealed record PollDto(
    Guid Id,
    Guid TenantId,
    Guid AuthorId,
    string AuthorName,
    string Title,
    string? Description,
    DateTime? ExpiresAtUtc,
    bool IsClosed,
    bool HasVoted,
    Guid? VotedOptionId,
    int TotalVotes,
    List<PollOptionDto> Options,
    DateTime CreatedAt);

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public sealed class CreatePollDto
{
    [Required(ErrorMessage = "O título da enquete é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
    public string? Description { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public List<string> Options { get; set; } = ["", ""];
}

public sealed class SubmitPollVoteDto
{
    [Required(ErrorMessage = "Selecione uma opção para votar.")]
    public Guid OptionId { get; set; }
}
