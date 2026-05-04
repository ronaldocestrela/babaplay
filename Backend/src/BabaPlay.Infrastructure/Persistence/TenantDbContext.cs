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
    public DbSet<GameDay> GameDays => Set<GameDay>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Checkin> Checkins => Set<Checkin>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<PlayerPosition> PlayerPositions => Set<PlayerPosition>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamPlayer> TeamPlayers => Set<TeamPlayer>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

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

        builder.Entity<GameDay>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.TenantId).IsRequired();
            e.Property(g => g.Name).IsRequired().HasMaxLength(120);
            e.Property(g => g.NormalizedName).IsRequired().HasMaxLength(120);
            e.Property(g => g.Location).HasMaxLength(200);
            e.Property(g => g.Description).HasMaxLength(500);
            e.Property(g => g.MaxPlayers).IsRequired();
            e.Property(g => g.Status).IsRequired();
            e.HasIndex(g => new { g.TenantId, g.NormalizedName, g.ScheduledAt }).IsUnique();
            e.HasIndex(g => new { g.TenantId, g.ScheduledAt });
        });

        builder.Entity<Match>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.TenantId).IsRequired();
            e.Property(m => m.GameDayId).IsRequired();
            e.Property(m => m.HomeTeamId).IsRequired();
            e.Property(m => m.AwayTeamId).IsRequired();
            e.Property(m => m.Description).HasMaxLength(500);
            e.Property(m => m.Status).IsRequired();
            e.Property(m => m.IsActive).IsRequired();
            e.HasIndex(m => new { m.TenantId, m.GameDayId, m.HomeTeamId, m.AwayTeamId }).IsUnique();
            e.HasIndex(m => new { m.TenantId, m.Status });
            e.HasIndex(m => new { m.TenantId, m.GameDayId });
        });

        builder.Entity<Checkin>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.TenantId).IsRequired();
            e.Property(c => c.PlayerId).IsRequired();
            e.Property(c => c.GameDayId).IsRequired();
            e.Property(c => c.CheckedInAtUtc).IsRequired();
            e.Property(c => c.Latitude).IsRequired();
            e.Property(c => c.Longitude).IsRequired();
            e.Property(c => c.DistanceFromAssociationMeters).IsRequired();
            e.Property(c => c.IsActive).IsRequired();
            e.HasIndex(c => new { c.TenantId, c.GameDayId, c.CheckedInAtUtc });
            e.HasIndex(c => new { c.TenantId, c.PlayerId, c.GameDayId, c.IsActive }).IsUnique();
        });

        builder.Entity<Position>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.TenantId).IsRequired();
            e.Property(p => p.Code).IsRequired().HasMaxLength(50);
            e.Property(p => p.NormalizedCode).IsRequired().HasMaxLength(50);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Description).HasMaxLength(300);
            e.HasIndex(p => new { p.TenantId, p.NormalizedCode }).IsUnique();
        });

        builder.Entity<PlayerPosition>(e =>
        {
            e.HasKey(pp => new { pp.PlayerId, pp.PositionId });

            e.HasOne<Player>()
                .WithMany(p => p.Positions)
                .HasForeignKey(pp => pp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Position>()
                .WithMany()
                .HasForeignKey(pp => pp.PositionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Team>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.TenantId).IsRequired();
            e.Property(t => t.Name).IsRequired().HasMaxLength(120);
            e.Property(t => t.NormalizedName).IsRequired().HasMaxLength(120);
            e.Property(t => t.MaxPlayers).IsRequired();
            e.HasIndex(t => new { t.TenantId, t.NormalizedName }).IsUnique();
        });

        builder.Entity<TeamPlayer>(e =>
        {
            e.HasKey(tp => new { tp.TeamId, tp.PlayerId });

            e.HasOne<Team>()
                .WithMany(t => t.Players)
                .HasForeignKey(tp => tp.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Player>()
                .WithMany()
                .HasForeignKey(tp => tp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Role>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).IsRequired().HasMaxLength(100);
            e.Property(r => r.NormalizedName).IsRequired().HasMaxLength(100);
            e.Property(r => r.Description).HasMaxLength(300);
            e.HasIndex(r => new { r.TenantId, r.NormalizedName }).IsUnique();
        });

        builder.Entity<Permission>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Code).IsRequired().HasMaxLength(120);
            e.Property(p => p.NormalizedCode).IsRequired().HasMaxLength(120);
            e.Property(p => p.Description).HasMaxLength(300);
            e.HasIndex(p => p.NormalizedCode).IsUnique();
        });

        builder.Entity<RolePermission>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            e.HasOne<Role>()
                .WithMany(r => r.Permissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Permission>()
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserRole>(e =>
        {
            e.HasKey(ur => new { ur.UserId, ur.RoleId });
            e.Property(ur => ur.UserId).IsRequired().HasMaxLength(450);

            e.HasOne<Role>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(builder);
    }
}
