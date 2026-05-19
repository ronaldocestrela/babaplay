using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// Permission repository backed by the per-tenant isolated database.
/// </summary>
public sealed class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PermissionRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Permission?> GetByNormalizedCodeAsync(string normalizedCode, CancellationToken ct = default)
    {
        var db = _db;
        return await db.Permissions.FirstOrDefaultAsync(
            p => p.TenantId == _tenantContext.TenantId && p.NormalizedCode == normalizedCode,
            ct);
    }

    public async Task AddAsync(Permission permission, CancellationToken ct = default)
    {
        if (permission.TenantId != _tenantContext.TenantId)
            throw new ValidationException("TenantId", "Permission tenant does not match request tenant context.");

        var db = _db;
        db.Permissions.Add(permission);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
