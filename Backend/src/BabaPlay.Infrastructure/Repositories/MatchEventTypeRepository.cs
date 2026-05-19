using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class MatchEventTypeRepository : IMatchEventTypeRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public MatchEventTypeRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<MatchEventType?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.MatchEventTypes.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<MatchEventType>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var db = _db;
        return await db.MatchEventTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNormalizedCodeAsync(string normalizedCode, Guid? excludeId, CancellationToken ct = default)
    {
        var db = _db;

        var query = db.MatchEventTypes
            .AsNoTracking()
            .Where(x => x.NormalizedCode == normalizedCode);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(MatchEventType matchEventType, CancellationToken ct = default)
    {
        var db = _db;
        db.MatchEventTypes.Add(matchEventType);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MatchEventType matchEventType, CancellationToken ct = default)
    {
        var db = _db;
        db.MatchEventTypes.Update(matchEventType);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
