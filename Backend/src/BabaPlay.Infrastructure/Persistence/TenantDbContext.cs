using BabaPlay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Per-tenant isolated database context.
/// Connection string is resolved dynamically per request via TenantDbContextFactory.
/// </summary>
public sealed class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Player>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Nickname).HasMaxLength(50);
            e.Property(p => p.Phone).HasMaxLength(20);
            e.HasIndex(p => p.UserId).IsUnique();
        });

        base.OnModelCreating(builder);
    }
}
