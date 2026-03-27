using BabaPlay.Modules.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class PlatformDbContext : DbContext, SharedKernel.Repositories.IPlatformUnitOfWork
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options) { }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<AllowedOrigin> AllowedOrigins => Set<AllowedOrigin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasIndex(x => x.Subdomain).IsUnique();
        });
        modelBuilder.Entity<Plan>(e => e.Property(x => x.MonthlyPrice).HasPrecision(18, 2));
        modelBuilder.Entity<AllowedOrigin>(e => e.HasIndex(x => x.Origin));
        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    Task<int> SharedKernel.Repositories.IPlatformUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        SaveChangesAsync(cancellationToken);
}
