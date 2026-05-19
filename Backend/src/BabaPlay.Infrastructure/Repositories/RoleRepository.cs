using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// Role repository backed by the per-tenant isolated database.
/// </summary>
public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public RoleRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<Role>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var db = _db;
        return await db.Roles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken ct = default)
    {
        var db = _db;
        return await db.Roles.AnyAsync(r => r.NormalizedName == normalizedName, ct);
    }

    public async Task AddAsync(Role role, CancellationToken ct = default)
    {
        if (role.TenantId != _tenantContext.TenantId)
            throw new ValidationException("TenantId", "Role tenant does not match request tenant context.");

        var db = _db;
        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        if (role.TenantId != _tenantContext.TenantId)
            throw new ValidationException("TenantId", "Role tenant does not match request tenant context.");

        var db = _db;
        db.Roles.Update(role);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
