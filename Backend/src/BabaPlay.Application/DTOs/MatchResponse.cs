using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

public sealed record MatchResponse(
    Guid Id,
    Guid TenantId,
    Guid GameDayId,
    Guid HomeTeamId,
    Guid AwayTeamId,
    string? Description,
    MatchStatus Status,
    bool IsActive,
    DateTime CreatedAt);