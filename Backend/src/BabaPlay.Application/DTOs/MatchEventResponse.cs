namespace BabaPlay.Application.DTOs;

public sealed record MatchEventResponse(
    Guid Id,
    Guid TenantId,
    Guid MatchId,
    Guid TeamId,
    Guid PlayerId,
    Guid MatchEventTypeId,
    int Minute,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt);
