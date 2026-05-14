using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a player-position association in a tenant.
/// </summary>
public sealed class PlayerPosition
{
    public Guid PlayerId { get; private set; }
    public Guid PositionId { get; private set; }

    private PlayerPosition() { }

    public static PlayerPosition Create(Guid playerId, Guid positionId)
    {
        if (playerId == Guid.Empty)
            throw new ValidationException("PlayerId", "PlayerId is required.");

        if (positionId == Guid.Empty)
            throw new ValidationException("PositionId", "PositionId is required.");

        return new PlayerPosition
        {
            PlayerId = playerId,
            PositionId = positionId,
        };
    }
}
