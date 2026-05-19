using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// User-role assignment repository backed by the per-tenant isolated database.
/// </summary>
public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public UserRoleRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<bool> ExistsAsync(string userId, Guid roleId, CancellationToken ct = default)
    {
        var db = _db;
        return await (
            from userRole in db.UserRoles
            join role in db.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
               && userRole.RoleId == roleId
            select userRole.RoleId
        ).AnyAsync(ct);
    }

    public async Task AddAsync(UserRole userRole, CancellationToken ct = default)
    {
        var db = _db;

        var roleTenantId = await db.Roles
            .Where(r => r.Id == userRole.RoleId)
            .Select(r => (Guid?)r.TenantId)
            .FirstOrDefaultAsync(ct);

        if (!roleTenantId.HasValue || roleTenantId.Value != _tenantContext.TenantId)
            throw new ValidationException("RoleId", "Role does not belong to request tenant context.");

        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> HasPermissionAsync(string userId, string normalizedPermissionCode, CancellationToken ct = default)
    {
        var db = _db;

        return await (
            from userRole in db.UserRoles
            join role in db.Roles on userRole.RoleId equals role.Id
            join rolePermission in db.RolePermissions on role.Id equals rolePermission.RoleId
            join permission in db.Permissions on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
               && role.IsActive
               && permission.NormalizedCode == normalizedPermissionCode
            select permission.Id
        ).AnyAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
