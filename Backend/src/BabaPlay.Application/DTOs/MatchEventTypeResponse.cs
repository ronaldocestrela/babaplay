namespace BabaPlay.Application.DTOs;

public sealed record MatchEventTypeResponse(
    Guid Id,
    Guid TenantId,
    string Code,
    string Name,
    int Points,
    bool IsSystemDefault,
    bool IsActive,
    DateTime CreatedAt);
