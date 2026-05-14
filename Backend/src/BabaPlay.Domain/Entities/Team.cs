using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a tenant-scoped team and its active roster.
/// </summary>
public sealed class Team : EntityBase
{
    private readonly List<TeamPlayer> _players = [];

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public int MaxPlayers { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Guid> PlayerIds => _players.Select(x => x.PlayerId).ToList().AsReadOnly();
    public IReadOnlyCollection<TeamPlayer> Players => _players.AsReadOnly();

    private Team() { }

    public static Team Create(Guid tenantId, string name, int maxPlayers)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Team name is required.");

        if (maxPlayers <= 0)
            throw new ValidationException("MaxPlayers", "MaxPlayers must be greater than zero.");

        var trimmedName = name.Trim();

        return new Team
        {
            TenantId = tenantId,
            Name = trimmedName,
            NormalizedName = NormalizeName(trimmedName),
            MaxPlayers = maxPlayers,
            IsActive = true,
        };
    }

    public void Update(string name, int maxPlayers)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Team name is required.");

        if (maxPlayers <= 0)
            throw new ValidationException("MaxPlayers", "MaxPlayers must be greater than zero.");

        Name = name.Trim();
        NormalizedName = NormalizeName(Name);
        MaxPlayers = maxPlayers;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    public void SetPlayers(IEnumerable<Guid>? playerIds, bool hasGoalkeeper)
    {
        if (playerIds is null)
        {
            _players.Clear();
            MarkUpdated();
            return;
        }

        var ids = playerIds.ToList();

        if (ids.Any(id => id == Guid.Empty))
            throw new ValidationException("PlayerIds", "PlayerId cannot be empty.");

        if (ids.Distinct().Count() != ids.Count)
            throw new ValidationException("PlayerIds", "PlayerIds cannot contain duplicates.");

        if (ids.Count > MaxPlayers)
            throw new ValidationException("PlayerIds", "Roster size exceeds team max players.");

        if (ids.Count > 0 && !hasGoalkeeper)
            throw new ValidationException("PlayerIds", "At least one goalkeeper is required in the roster.");

        _players.Clear();
        _players.AddRange(ids.Select(playerId => TeamPlayer.Create(Id, playerId)));
        MarkUpdated();
    }

    private static string NormalizeName(string name)
        => name.Trim().ToUpperInvariant();
}
