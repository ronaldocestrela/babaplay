using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

public sealed class TenantOwnerRbacBootstrapService : ITenantOwnerRbacBootstrapService
{
    private readonly AppDbContext _db;
    private readonly ITenantRbacCatalogSyncService _rbacCatalogSyncService;

    public TenantOwnerRbacBootstrapService(
        AppDbContext db,
        ITenantRbacCatalogSyncService rbacCatalogSyncService)
    {
        _db = db;
        _rbacCatalogSyncService = rbacCatalogSyncService;
    }

    public async Task<Result> EnsureOwnerAdminAccessAsync(string userId, Guid tenantId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Fail("TENANT_OWNER_USER_REQUIRED", "Tenant owner user is required.");

        if (tenantId == Guid.Empty)
            return Result.Fail("TENANT_ID_REQUIRED", "Tenant id is required.");

        var syncResult = await _rbacCatalogSyncService.SyncTenantAsync(tenantId, ct);
        if (!syncResult.IsSuccess)
            return syncResult;

        try
        {
            var adminRole = await _db.Roles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    r => r.TenantId == tenantId &&
                         r.NormalizedName == RbacCatalog.Roles.Admin.ToUpperInvariant(),
                    ct);

            if (adminRole is null)
                return Result.Fail("TENANT_ADMIN_ROLE_MISSING", "Admin role was not provisioned for tenant.");

            var ownerAlreadyAssigned = await _db.UserRoles
                .IgnoreQueryFilters()
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id, ct);

            if (!ownerAlreadyAssigned)
            {
                _db.UserRoles.Add(UserRole.Create(userId, adminRole.Id));
                await _db.SaveChangesAsync(ct);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail("TENANT_OWNER_RBAC_BOOTSTRAP_FAILED", ex.Message);
        }
    }
}
