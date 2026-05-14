using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// User-role assignment repository backed by the per-tenant isolated database.
/// </summary>
public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public UserRoleRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task<bool> ExistsAsync(string userId, Guid roleId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
    }

    public async Task AddAsync(UserRole userRole, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> HasPermissionAsync(string userId, string normalizedPermissionCode, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);

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
