using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// Position repository backed by the per-tenant isolated database.
/// </summary>
public sealed class PositionRepository : IPositionRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PositionRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Position?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.Positions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Position>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var db = _db;
        return await db.Positions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNormalizedCodeAsync(string normalizedCode, CancellationToken ct = default)
    {
        var db = _db;
        return await db.Positions.AnyAsync(p => p.NormalizedCode == normalizedCode, ct);
    }

    public async Task<bool> IsInUseAsync(Guid positionId, CancellationToken ct = default)
    {
        var db = _db;
        return await db.PlayerPositions.AnyAsync(pp => pp.PositionId == positionId, ct);
    }

    public async Task<IReadOnlyList<Position>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return [];

        var db = _db;
        return await db.Positions
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Position position, CancellationToken ct = default)
    {
        if (position.TenantId != _tenantContext.TenantId)
            throw new ValidationException("TenantId", "Position tenant does not match request tenant context.");

        var db = _db;
        db.Positions.Add(position);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Position position, CancellationToken ct = default)
    {
        if (position.TenantId != _tenantContext.TenantId)
            throw new ValidationException("TenantId", "Position tenant does not match request tenant context.");

        var db = _db;
        db.Positions.Update(position);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
