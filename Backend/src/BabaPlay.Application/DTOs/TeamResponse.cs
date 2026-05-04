namespace BabaPlay.Application.DTOs;

public sealed record TeamResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    int MaxPlayers,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<Guid> PlayerIds);
