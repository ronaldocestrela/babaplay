using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BabaPlay.Infrastructure.Workers;

/// <summary>
/// Background worker that processes tenant provisioning jobs.
/// For each job: marks tenant as InProgress, creates an isolated SQL Server database,
/// runs EF Core migrations, then marks tenant as Ready (or Failed on error).
/// </summary>
public sealed class TenantProvisioningWorker : BackgroundService
{
    private readonly ITenantProvisioningQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TenantProvisioningWorker> _logger;

    public TenantProvisioningWorker(
        ITenantProvisioningQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<TenantProvisioningWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TenantProvisioningWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            Guid tenantId;
            try
            {
                tenantId = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await ProvisionAsync(tenantId, stoppingToken);
        }

        _logger.LogInformation("TenantProvisioningWorker stopped.");
    }

    private async Task ProvisionAsync(Guid tenantId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

        _logger.LogInformation("Provisioning tenant {TenantId}...", tenantId);

        try
        {
            await tenantRepo.UpdateProvisioningAsync(tenantId, ProvisioningStatus.InProgress, string.Empty, ct);

            var tenant = await masterDb.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);
            if (tenant is null)
            {
                _logger.LogWarning("Tenant {TenantId} not found; skipping provisioning.", tenantId);
                return;
            }

            var masterConnectionString = masterDb.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Master connection string is unavailable.");

            var dbName = BuildDatabaseName(tenantId);
            var tenantConnectionString = BuildConnectionString(masterConnectionString, dbName);

            // Create isolated database and apply EF migrations
            var tenantOptions = new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlServer(tenantConnectionString)
                .Options;

            await using var tenantCtx = new TenantDbContext(tenantOptions);
            await tenantCtx.Database.MigrateAsync(ct);

            await tenantRepo.UpdateProvisioningAsync(tenantId, ProvisioningStatus.Ready, tenantConnectionString, ct);
            _logger.LogInformation("Tenant {TenantId} provisioned successfully (db: {DbName}).", tenantId, dbName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision tenant {TenantId}.", tenantId);
            try
            {
                await tenantRepo.UpdateProvisioningAsync(tenantId, ProvisioningStatus.Failed, string.Empty, ct);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Failed to mark tenant {TenantId} as Failed.", tenantId);
            }
        }
    }

    /// <summary>
    /// Derives the database name from the tenant id using a deterministic format.
    /// Example: BabaPlay_Tenant_3f2a1b0c4d5e6f7a8b9c0d1e2f3a4b5c
    /// </summary>
    internal static string BuildDatabaseName(Guid tenantId)
        => $"BabaPlay_Tenant_{tenantId:N}";

    /// <summary>
    /// Replaces the <c>Database=...</c> segment in the master connection string
    /// to target the tenant-specific database.
    /// </summary>
    internal static string BuildConnectionString(string masterConnectionString, string dbName)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = dbName
        };
        return builder.ConnectionString;
    }
}
