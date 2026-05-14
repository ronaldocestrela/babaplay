using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a player-team association in a tenant.
/// </summary>
public sealed class TeamPlayer
{
    public Guid TeamId { get; private set; }
    public Guid PlayerId { get; private set; }

    private TeamPlayer() { }

    public static TeamPlayer Create(Guid teamId, Guid playerId)
    {
        if (teamId == Guid.Empty)
            throw new ValidationException("TeamId", "TeamId is required.");

        if (playerId == Guid.Empty)
            throw new ValidationException("PlayerId", "PlayerId is required.");

        return new TeamPlayer
        {
            TeamId = teamId,
            PlayerId = playerId,
        };
    }
}
