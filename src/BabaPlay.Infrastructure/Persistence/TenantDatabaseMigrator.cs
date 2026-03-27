using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class TenantDatabaseMigrator : ITenantDatabaseMigrator
{
    public async Task MigrateAsync(string tenantConnectionString, CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(tenantConnectionString)
            .Options;
        await using var ctx = new TenantDbContext(options);
        await ctx.Database.MigrateAsync(cancellationToken);
    }
}
