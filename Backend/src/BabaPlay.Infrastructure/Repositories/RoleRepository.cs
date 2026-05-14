using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// Role repository backed by the per-tenant isolated database.
/// </summary>
public sealed class RoleRepository : IRoleRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public RoleRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<Role>> GetAllActiveAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Roles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Roles.AnyAsync(r => r.NormalizedName == normalizedName, ct);
    }

    public async Task AddAsync(Role role, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.Roles.Update(role);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
