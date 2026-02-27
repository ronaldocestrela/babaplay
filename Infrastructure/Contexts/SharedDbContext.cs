using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

/// <summary>
/// DbContext used for shared data that is not tenant-specific.  Currently it
/// only contains the allowed CORS origins table.  This context is configured
/// with the normal "DefaultConnection" string and is **not** multi-tenant.
/// </summary>
public class SharedDbContext : DbContext
{
    public SharedDbContext(DbContextOptions<SharedDbContext> options)
        : base(options)
    {
    }

    public DbSet<CorsOrigin> CorsOrigins => Set<CorsOrigin>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // only the CORS configuration applies here; avoid scanning the entire
        // assembly which would bring in Identity/tenant configurations that
        // are irrelevant to the shared context.
        builder.ApplyConfiguration(new DbConfigurations.AllowedCorsConfig());
    }
}
