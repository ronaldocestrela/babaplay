using System;
using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Web.Models;

// ─── Response DTOs ────────────────────────────────────────────────────────────

/// <summary>
/// Front-end DTO for an announcement, including per-user read status and freshness flag.
/// </summary>
public sealed record AnnouncementDto(
    Guid Id,
    Guid TenantId,
    Guid AuthorId,
    string AuthorName,
    string Title,
    string Content,
    bool IsPublished,
    DateTime? PublishedAtUtc,
    DateTime? ExpiresAtUtc,
    bool IsRead,
    bool IsNew,
    string? Tags,
    DateTime CreatedAt);

// ─── Request DTOs ─────────────────────────────────────────────────────────────

/// <summary>Payload for creating a new announcement.</summary>
public sealed class CreateAnnouncementDto
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(150, ErrorMessage = "Título deve ter no máximo 150 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "O conteúdo é obrigatório.")]
    [MaxLength(5000, ErrorMessage = "Conteúdo deve ter no máximo 5.000 caracteres.")]
    public string Content { get; set; } = string.Empty;

    public DateTime? ExpiresAtUtc { get; set; }

    [MaxLength(300, ErrorMessage = "Tags devem ter no máximo 300 caracteres.")]
    public string? Tags { get; set; }
}
