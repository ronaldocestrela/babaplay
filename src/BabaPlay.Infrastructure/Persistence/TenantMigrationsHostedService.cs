using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// On application startup, applies pending EF Core migrations to every tenant database listed in the platform catalog.
/// </summary>
public sealed class TenantMigrationsHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<DatabaseOptions> _dbOptions;
    private readonly TenantMigrationOrchestrator _orchestrator;
    private readonly ILogger<TenantMigrationsHostedService> _logger;

    public TenantMigrationsHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<DatabaseOptions> dbOptions,
        TenantMigrationOrchestrator orchestrator,
        ILogger<TenantMigrationsHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _dbOptions = dbOptions;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        _logger.LogInformation("Starting automatic tenant database migrations...");

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            var tenants = await platform.Tenants
                .AsNoTracking()
                .Select(t => new ValueTuple<string, string>(t.Subdomain, t.DatabaseName))
                .ToListAsync(stoppingToken);

            var platformCs = _dbOptions.Value.PlatformConnectionString;
            await _orchestrator.MigrateAllAsync(tenants, platformCs, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Automatic tenant database migrations could not be started (platform catalog unavailable?).");
        }
    }
}
