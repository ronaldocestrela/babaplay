using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

public sealed class TenantRbacCatalogSyncService : ITenantRbacCatalogSyncService
{
    private readonly AppDbContext _db;

    public TenantRbacCatalogSyncService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result> SyncAllTenantsAsync(CancellationToken ct = default)
    {
        var tenantIds = await _db.Tenants
            .AsNoTracking()
            .Where(t => t.IsActive)
            .Select(t => t.Id)
            .ToListAsync(ct);

        foreach (var tenantId in tenantIds)
        {
            var result = await SyncTenantAsync(tenantId, ct);
            if (!result.IsSuccess)
                return result;
        }

        return Result.Ok();
    }

    public async Task<Result> SyncTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        if (tenantId == Guid.Empty)
            return Result.Fail("TENANT_ID_REQUIRED", "Tenant id is required.");

        try
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var permissionByNormalizedCode = await _db.Permissions
                    .IgnoreQueryFilters()
                    .Where(p => p.TenantId == tenantId)
                    .ToDictionaryAsync(p => p.NormalizedCode, StringComparer.OrdinalIgnoreCase, ct);

                foreach (var permissionCode in RbacCatalog.AllPermissions)
                {
                    var normalizedCode = permissionCode.Trim().ToUpperInvariant();
                    if (permissionByNormalizedCode.ContainsKey(normalizedCode))
                        continue;

                    var permission = Permission.Create(
                        tenantId,
                        permissionCode,
                        $"System permission: {permissionCode}");

                    _db.Permissions.Add(permission);
                    permissionByNormalizedCode[normalizedCode] = permission;
                }

                await _db.SaveChangesAsync(ct);

                var roleByNormalizedName = await _db.Roles
                    .IgnoreQueryFilters()
                    .Include(r => r.Permissions)
                    .Where(r => r.TenantId == tenantId)
                    .ToDictionaryAsync(r => r.NormalizedName, StringComparer.OrdinalIgnoreCase, ct);

                foreach (var roleName in RbacCatalog.DefaultRolePermissions.Keys)
                {
                    var normalizedName = roleName.Trim().ToUpperInvariant();
                    if (roleByNormalizedName.ContainsKey(normalizedName))
                        continue;

                    var role = Role.Create(tenantId, roleName, "System default role");
                    _db.Roles.Add(role);
                    roleByNormalizedName[normalizedName] = role;
                }

                await _db.SaveChangesAsync(ct);

                foreach (var roleEntry in RbacCatalog.DefaultRolePermissions)
                {
                    var role = roleByNormalizedName[roleEntry.Key.Trim().ToUpperInvariant()];

                    foreach (var permissionCode in roleEntry.Value)
                    {
                        var permission = permissionByNormalizedCode[permissionCode.Trim().ToUpperInvariant()];
                        role.AddPermission(permission.Id);
                    }
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail("TENANT_RBAC_CATALOG_SYNC_FAILED", ex.Message);
        }
    }
}
