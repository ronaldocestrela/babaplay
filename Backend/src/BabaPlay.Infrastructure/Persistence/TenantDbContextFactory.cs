using BabaPlay.Domain.Exceptions;
using BabaPlay.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Creates a <see cref="TenantDbContext"/> pointing to the tenant's isolated database.
/// The connection string is looked up from the Master DB at runtime.
/// </summary>
public class TenantDbContextFactory
{
    private readonly MasterDbContext _masterDb;
    private readonly ITenantProvisioningMode _tenantProvisioningMode;

    public TenantDbContextFactory(MasterDbContext masterDb, ITenantProvisioningMode tenantProvisioningMode)
    {
        _masterDb = masterDb;
        _tenantProvisioningMode = tenantProvisioningMode;
    }

    /// <summary>
    /// Returns a <see cref="TenantDbContext"/> configured with the connection string
    /// for the given tenant. Throws <see cref="NotFoundException"/> if the tenant does
    /// not exist or its database has not been provisioned yet.
    /// </summary>
    public virtual async Task<TenantDbContext> CreateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _masterDb.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        if (tenant is null)
            throw new NotFoundException("TENANT_NOT_FOUND", $"Tenant '{tenantId}' was not found.");

        var connectionString = tenant.ConnectionString;

        if (!_tenantProvisioningMode.UseTenantDatabaseProvisioning)
        {
            connectionString = _masterDb.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Master connection string is unavailable.");
        }
        else if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new NotFoundException(
                "TENANT_NOT_PROVISIONED",
                $"Tenant '{tenant.Slug}' database has not been provisioned yet.");
        }

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TenantDbContext(options, tenantId);
    }
}
