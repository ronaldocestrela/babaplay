using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class MatchEventRepository : IMatchEventRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public MatchEventRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<MatchEvent?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.MatchEvents.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    }

    public async Task<IReadOnlyList<MatchEvent>> GetActiveByMatchAsync(Guid matchId, CancellationToken ct = default)
    {
        var db = _db;
        return await db.MatchEvents
            .AsNoTracking()
            .Where(x => x.IsActive && x.MatchId == matchId)
            .OrderBy(x => x.Minute)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MatchEvent>> GetActiveByPlayerAsync(Guid playerId, CancellationToken ct = default)
    {
        var db = _db;
        return await db.MatchEvents
            .AsNoTracking()
            .Where(x => x.IsActive && x.PlayerId == playerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(MatchEvent matchEvent, CancellationToken ct = default)
    {
        var db = _db;
        db.MatchEvents.Add(matchEvent);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MatchEvent matchEvent, CancellationToken ct = default)
    {
        var db = _db;
        db.MatchEvents.Update(matchEvent);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
