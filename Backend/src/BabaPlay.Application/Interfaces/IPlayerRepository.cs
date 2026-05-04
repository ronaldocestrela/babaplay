using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Persistence abstraction for player data stored in the Tenant database.
/// Implementations live in Infrastructure; Application depends only on this interface.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>Returns the player with the given id, or null if not found.</summary>
    Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all active players for the current tenant.</summary>
    Task<IReadOnlyList<Player>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>Returns true when a player with the given UserId already exists.</summary>
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns players by ids in the current tenant.</summary>
    Task<IReadOnlyList<Player>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    /// <summary>Tracks a new player for insertion on the next SaveChangesAsync call.</summary>
    Task AddAsync(Player player, CancellationToken ct = default);

    /// <summary>Marks a player as modified for update on the next SaveChangesAsync call.</summary>
    Task UpdateAsync(Player player, CancellationToken ct = default);

    /// <summary>Persists all pending changes to the tenant database.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
