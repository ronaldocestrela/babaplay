using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

public sealed class TenantOwnerRbacBootstrapService : ITenantOwnerRbacBootstrapService
{
    private readonly AppDbContext _db;

    public TenantOwnerRbacBootstrapService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result> EnsureOwnerAdminAccessAsync(string userId, Guid tenantId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Fail("TENANT_OWNER_USER_REQUIRED", "Tenant owner user is required.");

        if (tenantId == Guid.Empty)
            return Result.Fail("TENANT_ID_REQUIRED", "Tenant id is required.");

        try
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                var db = _db;
                await using var tx = await db.Database.BeginTransactionAsync(ct);

                var permissionByNormalizedCode = await db.Permissions
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

                    db.Permissions.Add(permission);
                    permissionByNormalizedCode[normalizedCode] = permission;
                }

                await db.SaveChangesAsync(ct);

                var roleByNormalizedName = await db.Roles
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
                    db.Roles.Add(role);
                    roleByNormalizedName[normalizedName] = role;
                }

                await db.SaveChangesAsync(ct);

                foreach (var roleEntry in RbacCatalog.DefaultRolePermissions)
                {
                    var role = roleByNormalizedName[roleEntry.Key.Trim().ToUpperInvariant()];

                    foreach (var permissionCode in roleEntry.Value)
                    {
                        var permission = permissionByNormalizedCode[permissionCode.Trim().ToUpperInvariant()];
                        role.AddPermission(permission.Id);
                    }
                }

                await db.SaveChangesAsync(ct);

                var adminRole = roleByNormalizedName[RbacCatalog.Roles.Admin.ToUpperInvariant()];
                var ownerAlreadyAssigned = await db.UserRoles
                    .IgnoreQueryFilters()
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id, ct);

                if (!ownerAlreadyAssigned)
                {
                    db.UserRoles.Add(UserRole.Create(userId, adminRole.Id));
                    await db.SaveChangesAsync(ct);
                }

                await tx.CommitAsync(ct);
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail("TENANT_OWNER_RBAC_BOOTSTRAP_FAILED", ex.Message);
        }
    }
}