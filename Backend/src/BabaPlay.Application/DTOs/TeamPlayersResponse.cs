namespace BabaPlay.Application.DTOs;

/// <summary>Data transfer object representing a team's assigned players.</summary>
public sealed record TeamPlayersResponse(
    Guid TeamId,
    IReadOnlyList<Guid> PlayerIds,
    DateTime? UpdatedAt);