using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// Player repository backed by the per-tenant isolated database.
/// A fresh <see cref="TenantDbContext"/> is created per operation via the factory.
/// </summary>
public sealed class PlayerRepository : IPlayerRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public PlayerRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Players
            .Include(p => p.Positions)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Player>> GetAllActiveAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
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
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Players.AnyAsync(p => p.UserId == userId, ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(Player player, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.Players.Add(player);
        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Player player, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);

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
