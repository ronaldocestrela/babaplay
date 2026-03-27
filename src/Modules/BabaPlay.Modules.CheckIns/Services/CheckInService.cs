using BabaPlay.Modules.CheckIns.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.CheckIns.Services;

public sealed class CheckInService
{
    private readonly ITenantRepository<CheckInSession> _sessions;
    private readonly ITenantRepository<CheckIn> _checkIns;
    private readonly ITenantUnitOfWork _uow;

    public CheckInService(
        ITenantRepository<CheckInSession> sessions,
        ITenantRepository<CheckIn> checkIns,
        ITenantUnitOfWork uow)
    {
        _sessions = sessions;
        _checkIns = checkIns;
        _uow = uow;
    }

    public async Task<Result<CheckInSession>> StartSessionAsync(string? createdByUserId, CancellationToken ct)
    {
        var session = new CheckInSession { CreatedByUserId = createdByUserId };
        await _sessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(session);
    }

    public async Task<Result<CheckIn>> RegisterCheckInAsync(string sessionId, string associateId, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(sessionId, ct);
        if (session is null) return Result.NotFound<CheckIn>("Session not found.");

        var day = DateTime.UtcNow.Date;
        var tomorrow = day.AddDays(1);
        var already = await _checkIns.Query().AnyAsync(
            c => c.AssociateId == associateId && c.CheckedInAt >= day && c.CheckedInAt < tomorrow,
            ct);
        if (already) return Result.Conflict<CheckIn>("Associate already checked in today.");

        var ci = new CheckIn { SessionId = sessionId, AssociateId = associateId, CheckedInAt = DateTime.UtcNow };
        await _checkIns.AddAsync(ci, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(ci);
    }

    public async Task<Result<IReadOnlyList<CheckIn>>> ListForSessionAsync(string sessionId, CancellationToken ct)
    {
        var list = await _checkIns.Query().Where(c => c.SessionId == sessionId).OrderBy(c => c.CheckedInAt)
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<CheckIn>>(list);
    }
}
