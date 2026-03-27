namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Applies EF Core migrations for <see cref="TenantDbContext"/> against a tenant database.
/// </summary>
public interface ITenantDatabaseMigrator
{
    Task MigrateAsync(string tenantConnectionString, CancellationToken cancellationToken = default);
}
