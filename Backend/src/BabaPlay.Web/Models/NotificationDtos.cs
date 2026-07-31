using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Web.Models;

public sealed record NotificationDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    int Type,
    string Title,
    string Message,
    string? PayloadJson,
    bool IsRead,
    DateTime? ReadAtUtc,
    DateTime CreatedAt);

public sealed record NotificationSummaryDto(
    int UnreadCount,
    List<NotificationDto> Notifications);

public sealed record SendNotificationResultDto(int CreatedCount);

public sealed class CreateNotificationDto
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A mensagem é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A mensagem deve ter no máximo 5000 caracteres.")]
    public string Message { get; set; } = string.Empty;

    public int Type { get; set; } = 4;
}
