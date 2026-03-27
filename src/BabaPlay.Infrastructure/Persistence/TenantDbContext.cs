using BabaPlay.Modules.Associates.Entities;
using BabaPlay.Modules.Associations.Entities;
using BabaPlay.Modules.CheckIns.Entities;
using BabaPlay.Modules.Financial.Entities;
using BabaPlay.Modules.Identity;
using BabaPlay.Modules.Identity.Entities;
using BabaPlay.Modules.TeamGeneration.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class TenantDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, SharedKernel.Repositories.ITenantUnitOfWork
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<Association> Associations => Set<Association>();
    public DbSet<Associate> Associates => Set<Associate>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<AssociatePosition> AssociatePositions => Set<AssociatePosition>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<CheckInSession> CheckInSessions => Set<CheckInSession>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CashEntry> CashEntries => Set<CashEntry>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Associate>(e =>
        {
            e.HasMany(x => x.Positions).WithOne(x => x.Associate).HasForeignKey(x => x.AssociateId);
        });
        modelBuilder.Entity<AssociatePosition>(e =>
        {
            e.HasIndex(x => new { x.AssociateId, x.PositionId }).IsUnique();
            e.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId);
        });
        modelBuilder.Entity<CheckIn>(e =>
        {
            e.HasOne(x => x.Session).WithMany(x => x.CheckIns).HasForeignKey(x => x.SessionId);
            e.HasIndex(x => new { x.AssociateId, x.CheckedInAt });
        });
        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
            e.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId);
        });
        modelBuilder.Entity<CashEntry>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId);
        });
        modelBuilder.Entity<Membership>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.AssociateId, x.Year, x.Month }).IsUnique();
        });
        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Membership).WithMany(x => x.Payments).HasForeignKey(x => x.MembershipId);
        });
        modelBuilder.Entity<TeamMember>(e =>
        {
            e.HasOne(x => x.Team).WithMany(x => x.Members).HasForeignKey(x => x.TeamId);
        });
    }

    Task<int> SharedKernel.Repositories.ITenantUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        SaveChangesAsync(cancellationToken);
}
