using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BabaPlay.Infrastructure.Security;

public sealed class AllowedOriginsSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AllowedOriginsCache _cache;
    private readonly ILogger<AllowedOriginsSyncWorker> _logger;

    public AllowedOriginsSyncWorker(
        IServiceScopeFactory scopeFactory,
        AllowedOriginsCache cache,
        ILogger<AllowedOriginsSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
                var list = await db.AllowedOrigins.AsNoTracking().Select(x => x.Origin).ToListAsync(stoppingToken);
                _cache.ReplaceAll(list);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh allowed origins cache");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
