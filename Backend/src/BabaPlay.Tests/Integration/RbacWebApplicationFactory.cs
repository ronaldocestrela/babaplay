using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// WebApplicationFactory dedicated to RBAC integration scenarios.
/// </summary>
public sealed class RbacWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public const string TenantASlug = "rbac-tenant-a";
    public const string TenantBSlug = "rbac-tenant-b";

    public const string AdminUserId = "rbac-admin-user";
    public const string MemberUserId = "rbac-member-user";
    public const string StrangerUserId = "rbac-stranger-user";

    private readonly SqliteConnection _masterConnection = new("Data Source=:memory:");
    private readonly SqliteConnection _tenantConnection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "rbac-integration-secret-key-exactly-256-bits-long-xxxxxxxxxxxxxxxxxxxxxxx",
                ["Jwt:Issuer"] = "BabaPlay.Api",
                ["Jwt:Audience"] = "BabaPlay.Clients",
                ["Jwt:AccessTokenExpiresInMinutes"] = "60",
                ["Jwt:RefreshTokenExpiresInDays"] = "30",
                ["ConnectionStrings:MasterDb"] = "Data Source=:memory:",
            });
        });

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<MasterDbContext>) ||
                    d.ServiceType == typeof(MasterDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().FullName ==
                         "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration`1" &&
                     d.ServiceType.GenericTypeArguments.FirstOrDefault() == typeof(MasterDbContext)) ||
                    (d.ImplementationType?.Assembly.GetName().Name?
                        .Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true) ||
                    (d.ImplementationInstance?.GetType().Assembly.GetName().Name?
                        .Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true))
                .ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            _masterConnection.Open();
            services.AddDbContext<MasterDbContext>(o => o.UseSqlite(_masterConnection));

            var factoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TenantDbContextFactory));
            if (factoryDescriptor is not null)
                services.Remove(factoryDescriptor);

            _tenantConnection.Open();
            services.AddScoped<TenantDbContextFactory>(_ => new TestTenantDbContextFactory(_tenantConnection));

            var queueDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITenantProvisioningQueue));
            if (queueDescriptor is not null)
                services.Remove(queueDescriptor);

            services.AddSingleton<ITenantProvisioningQueue, NoOpProvisioningQueue>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            masterDb.Database.EnsureCreated();
            SeedMasterAsync(masterDb, userManager).GetAwaiter().GetResult();

            var tenantFactory = scope.ServiceProvider.GetRequiredService<TenantDbContextFactory>();
            SeedTenantAsync(tenantFactory).GetAwaiter().GetResult();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _masterConnection.Dispose();
            _tenantConnection.Dispose();
        }
    }

    private static async Task SeedMasterAsync(MasterDbContext db, UserManager<ApplicationUser> userManager)
    {
        var users = new[]
        {
            new { Id = AdminUserId, Email = "rbac-admin@babaplay.com" },
            new { Id = MemberUserId, Email = "rbac-member@babaplay.com" },
            new { Id = StrangerUserId, Email = "rbac-stranger@babaplay.com" },
        };

        foreach (var userData in users)
        {
            if (await userManager.FindByIdAsync(userData.Id) is not null)
                continue;

            var user = new ApplicationUser
            {
                Id = userData.Id,
                UserName = userData.Email,
                Email = userData.Email,
                EmailConfirmed = true,
                IsActive = true,
            };

            var createResult = await userManager.CreateAsync(user, "RbacTest@123456");
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Seed user '{userData.Id}' failed: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!db.Tenants.Any(t => t.Id == TenantAId))
        {
            db.Tenants.Add(new Tenant
            {
                Id = TenantAId,
                Name = "RBAC Tenant A",
                Slug = TenantASlug,
                IsActive = true,
                ProvisioningStatus = ProvisioningStatus.Ready,
                ConnectionString = "placeholder-overridden-by-test-factory",
            });
        }

        if (!db.Tenants.Any(t => t.Id == TenantBId))
        {
            db.Tenants.Add(new Tenant
            {
                Id = TenantBId,
                Name = "RBAC Tenant B",
                Slug = TenantBSlug,
                IsActive = true,
                ProvisioningStatus = ProvisioningStatus.Ready,
                ConnectionString = "placeholder-overridden-by-test-factory",
            });
        }

        if (!db.UserTenants.Any(ut => ut.UserId == AdminUserId && ut.TenantId == TenantAId))
        {
            db.UserTenants.Add(new UserTenant
            {
                UserId = AdminUserId,
                TenantId = TenantAId,
                IsOwner = true,
            });
        }

        if (!db.UserTenants.Any(ut => ut.UserId == MemberUserId && ut.TenantId == TenantAId))
        {
            db.UserTenants.Add(new UserTenant
            {
                UserId = MemberUserId,
                TenantId = TenantAId,
                IsOwner = false,
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedTenantAsync(TenantDbContextFactory factory)
    {
        await using var db = await factory.CreateAsync(TenantAId);
        await db.Database.EnsureCreatedAsync();

        var roleReadPermission = await db.Permissions.FirstOrDefaultAsync(p => p.NormalizedCode == RbacCatalog.Permissions.RbacRolesRead.ToUpperInvariant());
        if (roleReadPermission is null)
        {
            roleReadPermission = Permission.Create(RbacCatalog.Permissions.RbacRolesRead, "Read tenant roles");
            db.Permissions.Add(roleReadPermission);
            await db.SaveChangesAsync();
        }

        var rankingReadPermission = await db.Permissions.FirstOrDefaultAsync(p => p.NormalizedCode == RbacCatalog.Permissions.RankingRead.ToUpperInvariant());
        if (rankingReadPermission is null)
        {
            rankingReadPermission = Permission.Create(RbacCatalog.Permissions.RankingRead, "Read ranking");
            db.Permissions.Add(rankingReadPermission);
            await db.SaveChangesAsync();
        }

        var rankingWritePermission = await db.Permissions.FirstOrDefaultAsync(p => p.NormalizedCode == RbacCatalog.Permissions.RankingWrite.ToUpperInvariant());
        if (rankingWritePermission is null)
        {
            rankingWritePermission = Permission.Create(RbacCatalog.Permissions.RankingWrite, "Write ranking");
            db.Permissions.Add(rankingWritePermission);
            await db.SaveChangesAsync();
        }

        var adminRole = await db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.NormalizedName == RbacCatalog.Roles.Admin.ToUpperInvariant());

        if (adminRole is null)
        {
            adminRole = Role.Create(TenantAId, RbacCatalog.Roles.Admin, "Admin role for integration tests");
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync();
        }

        adminRole.AddPermission(roleReadPermission.Id);
        adminRole.AddPermission(rankingReadPermission.Id);
        adminRole.AddPermission(rankingWritePermission.Id);

        var viewerRole = await db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.NormalizedName == RbacCatalog.Roles.Viewer.ToUpperInvariant());

        if (viewerRole is null)
        {
            viewerRole = Role.Create(TenantAId, RbacCatalog.Roles.Viewer, "Viewer role for integration tests");
            db.Roles.Add(viewerRole);
            await db.SaveChangesAsync();
        }

        if (!await db.UserRoles.AnyAsync(ur => ur.UserId == AdminUserId && ur.RoleId == adminRole.Id))
            db.UserRoles.Add(UserRole.Create(AdminUserId, adminRole.Id));

        if (!await db.UserRoles.AnyAsync(ur => ur.UserId == MemberUserId && ur.RoleId == viewerRole.Id))
            db.UserRoles.Add(UserRole.Create(MemberUserId, viewerRole.Id));

        await db.SaveChangesAsync();
    }

    private sealed class TestTenantDbContextFactory : TenantDbContextFactory
    {
        private readonly SqliteConnection _connection;

        public TestTenantDbContextFactory(SqliteConnection connection)
            : base(null!)
        {
            _connection = connection;
        }

        public override Task<TenantDbContext> CreateAsync(Guid tenantId, CancellationToken ct = default)
        {
            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlite(_connection)
                .Options;

            return Task.FromResult(new TenantDbContext(options));
        }
    }

    private sealed class NoOpProvisioningQueue : ITenantProvisioningQueue
    {
        public Task EnqueueAsync(Guid tenantId, CancellationToken ct = default) => Task.CompletedTask;

        public Task<Guid> DequeueAsync(CancellationToken ct = default)
        {
            var tcs = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs.Task;
        }
    }
}
