using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// Player repository backed by the unified app database with tenant query filters.
/// </summary>
public sealed class PlayerRepository : IPlayerRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PlayerRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.Players
            .Include(p => p.Positions)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Player>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var db = _db;
        return await db.Players
            .Include(p => p.Positions)
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var db = _db;
        return await db.Players.AnyAsync(p => p.UserId == userId, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Player>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return [];

        var db = _db;
        return await db.Players
            .Include(p => p.Positions)
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(Player player, CancellationToken ct = default)
    {
        if (player.TenantId != _tenantContext.TenantId)
            throw new ValidationException("TenantId", "Player tenant does not match request tenant context.");

        var db = _db;
        db.Players.Add(player);
        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Player player, CancellationToken ct = default)
    {
        var db = _db;

        var existing = await db.Players
            .Include(p => p.Positions)
            .FirstOrDefaultAsync(p => p.Id == player.Id, ct);

        if (existing is null)
        {
            db.Players.Update(player);
            await db.SaveChangesAsync(ct);
            return;
        }

        db.Entry(existing).CurrentValues.SetValues(player);

        db.PlayerPositions.RemoveRange(existing.Positions);
        foreach (var positionId in player.PositionIds)
            db.PlayerPositions.Add(PlayerPosition.Create(existing.Id, positionId));

        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken ct = default)
        // No-op: each operation above already persists. SaveChangesAsync on the
        // repository interface exists to allow unit tests to verify the call was made.
        => Task.CompletedTask;
}
