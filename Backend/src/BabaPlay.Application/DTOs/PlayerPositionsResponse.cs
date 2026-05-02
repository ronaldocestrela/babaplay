namespace BabaPlay.Application.DTOs;

/// <summary>Data transfer object representing a player's assigned positions.</summary>
public sealed record PlayerPositionsResponse(
    Guid PlayerId,
    IReadOnlyList<Guid> PositionIds,
    DateTime? UpdatedAt);
