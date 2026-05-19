using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.ValueObjects;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class PlayerScoreRepository : IPlayerScoreRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PlayerScoreRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<PlayerScore?> GetByPlayerIdAsync(Guid playerId, CancellationToken ct = default)
    {
        var db = _db;

        return await db.PlayerScores
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.PlayerId == playerId && ps.IsActive, ct);
    }

    public async Task<IReadOnlyList<PlayerScore>> GetRankingAsync(RankingPeriod? period, int skip, int take, CancellationToken ct = default)
    {
        var db = _db;

        var query = ApplyPeriodFilter(db.PlayerScores.AsNoTracking().Where(ps => ps.IsActive), period)
            .OrderByDescending(ps => ps.ScoreTotal)
            .ThenByDescending(ps => ps.Goals)
            .ThenByDescending(ps => ps.AttendanceCount)
            .ThenBy(ps => ps.PlayerId)
            .Skip(skip)
            .Take(take);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlayerScore>> GetTopScorersAsync(RankingPeriod? period, int skip, int take, CancellationToken ct = default)
    {
        var db = _db;

        var query = ApplyPeriodFilter(db.PlayerScores.AsNoTracking().Where(ps => ps.IsActive), period)
            .OrderByDescending(ps => ps.Goals)
            .ThenByDescending(ps => ps.ScoreTotal)
            .ThenByDescending(ps => ps.AttendanceCount)
            .ThenBy(ps => ps.PlayerId)
            .Skip(skip)
            .Take(take);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlayerScore>> GetAttendanceRankingAsync(RankingPeriod? period, int skip, int take, CancellationToken ct = default)
    {
        var db = _db;

        var query = ApplyPeriodFilter(db.PlayerScores.AsNoTracking().Where(ps => ps.IsActive), period)
            .OrderByDescending(ps => ps.AttendanceCount)
            .ThenByDescending(ps => ps.ScoreTotal)
            .ThenByDescending(ps => ps.Goals)
            .ThenBy(ps => ps.PlayerId)
            .Skip(skip)
            .Take(take);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlayerScore>> GetAllActiveForRebuildAsync(RankingPeriod? period, CancellationToken ct = default)
    {
        var db = _db;

        var query = ApplyPeriodFilter(db.PlayerScores.AsNoTracking().Where(ps => ps.IsActive), period)
            .OrderBy(ps => ps.PlayerId);

        return await query.ToListAsync(ct);
    }

    public async Task<bool> HasProcessedSourceEventAsync(Guid sourceEventId, CancellationToken ct = default)
    {
        var db = _db;

        return await db.PlayerScoreSourceEvents
            .AsNoTracking()
            .AnyAsync(x => x.SourceEventId == sourceEventId, ct);
    }

    public async Task AddProcessedSourceEventAsync(PlayerScoreSourceEvent sourceEvent, CancellationToken ct = default)
    {
        var db = _db;
        db.PlayerScoreSourceEvents.Add(sourceEvent);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddAsync(PlayerScore playerScore, CancellationToken ct = default)
    {
        var db = _db;
        db.PlayerScores.Add(playerScore);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PlayerScore playerScore, CancellationToken ct = default)
    {
        var db = _db;
        db.PlayerScores.Update(playerScore);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;

    private static IQueryable<PlayerScore> ApplyPeriodFilter(IQueryable<PlayerScore> query, RankingPeriod? period)
    {
        if (!period.HasValue)
            return query;

        var fromUtc = period.Value.FromUtc;
        var toUtc = period.Value.ToUtc;

        return query.Where(ps => (ps.UpdatedAt ?? ps.CreatedAt) >= fromUtc && (ps.UpdatedAt ?? ps.CreatedAt) <= toUtc);
    }
}