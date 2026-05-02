using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Per-tenant isolated database context.
/// Phase 2: structure only — entities (Players, Matches, etc.) added in Phase 3+.
/// Connection string is resolved dynamically per request via TenantDbContextFactory.
/// </summary>
public sealed class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Tenant-specific aggregates configured here in Phase 3+.
        base.OnModelCreating(builder);
    }
}
