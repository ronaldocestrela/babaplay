using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class MatchSummaryRepository : IMatchSummaryRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public MatchSummaryRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<MatchSummary?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.MatchSummaries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    }

    public async Task<MatchSummary?> GetByMatchIdAsync(Guid matchId, CancellationToken ct = default)
    {
        var db = _db;
        return await db.MatchSummaries.AsNoTracking().FirstOrDefaultAsync(x => x.MatchId == matchId && x.IsActive, ct);
    }

    public async Task AddAsync(MatchSummary summary, CancellationToken ct = default)
    {
        var db = _db;
        db.MatchSummaries.Add(summary);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MatchSummary summary, CancellationToken ct = default)
    {
        var db = _db;
        db.MatchSummaries.Update(summary);
        await db.SaveChangesAsync(ct);
    }

    // No-op because AddAsync/UpdateAsync create isolated contexts and persist immediately.
    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
