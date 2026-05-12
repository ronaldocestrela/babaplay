using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

public sealed class PlayerOnboardingReadService : IPlayerOnboardingReadService
{
    private readonly TenantDbContextFactory _tenantDbContextFactory;

    public PlayerOnboardingReadService(TenantDbContextFactory tenantDbContextFactory)
    {
        _tenantDbContextFactory = tenantDbContextFactory;
    }

    public async Task<bool> HasActivePlayerProfileAsync(Guid tenantId, string userId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return false;

        await using var db = await _tenantDbContextFactory.CreateAsync(tenantId, ct);

        return await db.Players
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userGuid && p.IsActive, ct);
    }
}
