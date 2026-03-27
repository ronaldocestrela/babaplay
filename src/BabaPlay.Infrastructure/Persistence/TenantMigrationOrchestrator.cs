using Microsoft.Extensions.Logging;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Applies pending tenant migrations for every registered tenant database.
/// </summary>
public sealed class TenantMigrationOrchestrator
{
    private readonly ITenantDatabaseMigrator _migrator;
    private readonly ILogger<TenantMigrationOrchestrator> _logger;

    public TenantMigrationOrchestrator(ITenantDatabaseMigrator migrator, ILogger<TenantMigrationOrchestrator> logger)
    {
        _migrator = migrator;
        _logger = logger;
    }

    public async Task<TenantMigrationBatchResult> MigrateAllAsync(
        IReadOnlyList<(string Subdomain, string DatabaseName)> tenants,
        string platformConnectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(platformConnectionString))
        {
            _logger.LogWarning("Platform connection string is empty; skipping tenant migrations.");
            return new TenantMigrationBatchResult(0, 0, 0, 0);
        }

        var migrated = 0;
        var failed = 0;
        var skippedEmptyDb = 0;

        foreach (var (subdomain, databaseName) in tenants)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                skippedEmptyDb++;
                _logger.LogWarning("Skipping tenant {Subdomain}: empty DatabaseName.", subdomain);
                continue;
            }

            var tenantCs = TenantConnectionStringFactory.ForDatabase(platformConnectionString, databaseName);
            try
            {
                await _migrator.MigrateAsync(tenantCs, cancellationToken);
                migrated++;
                _logger.LogInformation(
                    "Tenant database migrated: subdomain={Subdomain} database={Database}",
                    subdomain,
                    databaseName);
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogError(
                    ex,
                    "Tenant database migration failed: subdomain={Subdomain} database={Database}",
                    subdomain,
                    databaseName);
            }
        }

        _logger.LogInformation(
            "Tenant migrations finished: total={Total} migrated={Migrated} failed={Failed} skippedEmptyDb={Skipped}",
            tenants.Count,
            migrated,
            failed,
            skippedEmptyDb);

        return new TenantMigrationBatchResult(tenants.Count, migrated, failed, skippedEmptyDb);
    }
}
