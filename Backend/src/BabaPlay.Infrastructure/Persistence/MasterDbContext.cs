using BabaPlay.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Master database context — shared across all tenants.
/// Owns: Identity (users/roles), Tenants, Subscriptions, Plans, RefreshTokens.
/// Tenant-specific data lives in per-tenant databases (Phase 2).
/// </summary>
public sealed class MasterDbContext : IdentityDbContext<ApplicationUser>
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Plan> Plans => Set<Plan>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
            e.Ignore(r => r.IsRevoked); // computed, not persisted
            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.ConnectionString).HasMaxLength(2000);
            e.Property(t => t.LogoPath).HasMaxLength(1024);
            e.Property(t => t.Street).HasMaxLength(160);
            e.Property(t => t.Number).HasMaxLength(30);
            e.Property(t => t.Neighborhood).HasMaxLength(120);
            e.Property(t => t.City).HasMaxLength(100);
            e.Property(t => t.State).HasMaxLength(100);
            e.Property(t => t.ZipCode).HasMaxLength(20);
            e.Property(t => t.AssociationLatitude);
            e.Property(t => t.AssociationLongitude);
            e.Property(t => t.CheckinRadiusMeters);
        });

        builder.Entity<UserTenant>(e =>
        {
            e.HasKey(ut => new { ut.UserId, ut.TenantId });
            e.HasOne(ut => ut.User).WithMany().HasForeignKey(ut => ut.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ut => ut.Tenant).WithMany(t => t.UserTenants).HasForeignKey(ut => ut.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Plan>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
        });

        builder.Entity<Subscription>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Tenant).WithMany(t => t.Subscriptions).HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Plan).WithMany(p => p.Subscriptions).HasForeignKey(s => s.PlanId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
