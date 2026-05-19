using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

public sealed class PlayerOnboardingReadService : IPlayerOnboardingReadService
{
    private readonly AppDbContext _db;

    public PlayerOnboardingReadService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasActivePlayerProfileAsync(Guid tenantId, string userId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return false;

        return await _db.Players
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(p => p.TenantId == tenantId && p.UserId == userGuid && p.IsActive, ct);
    }
}
