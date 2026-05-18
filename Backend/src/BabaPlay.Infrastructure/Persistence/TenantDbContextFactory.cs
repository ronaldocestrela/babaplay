using BabaPlay.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Creates a <see cref="TenantDbContext"/> pointing to the tenant's isolated database.
/// The connection string is looked up from the Master DB at runtime.
/// </summary>
public class TenantDbContextFactory
{
    private readonly MasterDbContext _masterDb;

    public TenantDbContextFactory(MasterDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    /// <summary>
    /// Returns a <see cref="TenantDbContext"/> using the shared master connection string.
    /// Throws <see cref="NotFoundException"/> when the tenant metadata does not exist.
    /// </summary>
    public virtual async Task<TenantDbContext> CreateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _masterDb.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        if (tenant is null)
            throw new NotFoundException("TENANT_NOT_FOUND", $"Tenant '{tenantId}' was not found.");

        var connectionString = _masterDb.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Master connection string is unavailable.");

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TenantDbContext(options, tenantId);
    }
}
