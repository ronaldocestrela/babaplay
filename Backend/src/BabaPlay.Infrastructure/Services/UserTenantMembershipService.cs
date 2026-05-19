using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

public sealed class UserTenantMembershipService : IUserTenantMembershipService
{
    private readonly AppDbContext _masterDbContext;

    public UserTenantMembershipService(AppDbContext masterDbContext)
    {
        _masterDbContext = masterDbContext;
    }

    public async Task<bool> EnsureMemberAsync(string userId, Guid tenantId, CancellationToken ct = default)
    {
        var existing = await _masterDbContext.UserTenants
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId, ct);

        if (existing is not null)
            return true;

        _masterDbContext.UserTenants.Add(new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            IsOwner = false,
            JoinedAt = DateTime.UtcNow,
        });

        await _masterDbContext.SaveChangesAsync(ct);
        return false;
    }
}
