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
    public DbSet<MatchSummary> MatchSummaries => Set<MatchSummary>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<MatchEventType> MatchEventTypes => Set<MatchEventType>();
    public DbSet<Checkin> Checkins => Set<Checkin>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<PlayerPosition> PlayerPositions => Set<PlayerPosition>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamPlayer> TeamPlayers => Set<TeamPlayer>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<PlayerScore> PlayerScores => Set<PlayerScore>();
    public DbSet<PlayerScoreSourceEvent> PlayerScoreSourceEvents => Set<PlayerScoreSourceEvent>();

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

        builder.Entity<MatchSummary>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TenantId).IsRequired();
            e.Property(x => x.MatchId).IsRequired();
            e.Property(x => x.StoragePath).IsRequired().HasMaxLength(400);
            e.Property(x => x.FileName).IsRequired().HasMaxLength(150);
            e.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
            e.Property(x => x.SizeBytes).IsRequired();
            e.Property(x => x.GeneratedAtUtc).IsRequired();
            e.Property(x => x.IsActive).IsRequired();
            e.HasIndex(x => new { x.TenantId, x.MatchId }).IsUnique();

            e.HasOne<Match>()
                .WithMany()
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MatchEventType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TenantId).IsRequired();
            e.Property(x => x.Code).IsRequired().HasMaxLength(50);
            e.Property(x => x.NormalizedCode).IsRequired().HasMaxLength(50);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Points).IsRequired();
            e.Property(x => x.IsSystemDefault).IsRequired();
            e.Property(x => x.IsActive).IsRequired();
            e.HasIndex(x => new { x.TenantId, x.NormalizedCode }).IsUnique();
        });

        builder.Entity<MatchEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TenantId).IsRequired();
            e.Property(x => x.MatchId).IsRequired();
            e.Property(x => x.TeamId).IsRequired();
            e.Property(x => x.PlayerId).IsRequired();
            e.Property(x => x.MatchEventTypeId).IsRequired();
            e.Property(x => x.Minute).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.IsActive).IsRequired();
            e.HasIndex(x => new { x.TenantId, x.MatchId, x.Minute });
            e.HasIndex(x => new { x.TenantId, x.PlayerId, x.IsActive });

            e.HasOne<Match>()
                .WithMany()
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Team>()
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Player>()
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<MatchEventType>()
                .WithMany()
                .HasForeignKey(x => x.MatchEventTypeId)
                .OnDelete(DeleteBehavior.Restrict);
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

        builder.Entity<PlayerScore>(e =>
        {
            e.HasKey(ps => ps.Id);
            e.Property(ps => ps.TenantId).IsRequired();
            e.Property(ps => ps.PlayerId).IsRequired();
            e.Property(ps => ps.AttendanceCount).IsRequired();
            e.Property(ps => ps.Wins).IsRequired();
            e.Property(ps => ps.Draws).IsRequired();
            e.Property(ps => ps.Goals).IsRequired();
            e.Property(ps => ps.YellowCards).IsRequired();
            e.Property(ps => ps.RedCards).IsRequired();
            e.Property(ps => ps.ScoreTotal).IsRequired();
            e.Property(ps => ps.IsActive).IsRequired();

            e.HasIndex(ps => new { ps.TenantId, ps.PlayerId }).IsUnique();
            e.HasIndex(ps => new { ps.TenantId, ps.ScoreTotal });
            e.HasIndex(ps => new { ps.TenantId, ps.Goals });
            e.HasIndex(ps => new { ps.TenantId, ps.AttendanceCount });

            e.HasOne<Player>()
                .WithMany()
                .HasForeignKey(ps => ps.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PlayerScoreSourceEvent>(e =>
        {
            e.HasKey(se => se.Id);
            e.Property(se => se.TenantId).IsRequired();
            e.Property(se => se.SourceEventId).IsRequired();
            e.Property(se => se.PlayerId).IsRequired();
            e.Property(se => se.AppliedAtUtc).IsRequired();

            e.HasIndex(se => new { se.TenantId, se.SourceEventId }).IsUnique();
            e.HasIndex(se => new { se.TenantId, se.PlayerId });

            e.HasOne<Player>()
                .WithMany()
                .HasForeignKey(se => se.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(builder);
    }
}
