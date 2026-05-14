namespace BabaPlay.Application.DTOs;

/// <summary>Data transfer object representing a player's public profile.</summary>
public sealed record PlayerResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string? Nickname,
    string? Phone,
    DateOnly? DateOfBirth,
    IReadOnlyList<Guid> PositionIds,
    bool IsActive,
    DateTime CreatedAt);
