using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

/// <summary>
/// Applies pending EF Core migrations using the unified AppDbContext.
/// </summary>
public sealed class DatabaseMigrationRunner
{
    private readonly AppDbContext _appDb;

    public DatabaseMigrationRunner(AppDbContext appDb)
    {
        _appDb = appDb;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        if (!_appDb.Database.IsRelational())
            throw new InvalidOperationException("AppDbContext must use a relational provider for migrations.");

        await _appDb.Database.MigrateAsync(ct);
    }
}