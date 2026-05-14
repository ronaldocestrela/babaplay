using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

public sealed record GameDayResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    DateTime ScheduledAt,
    string? Location,
    string? Description,
    int MaxPlayers,
    GameDayStatus Status,
    bool IsActive,
    DateTime CreatedAt);
