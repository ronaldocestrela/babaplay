using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Common;
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
/// WebApplicationFactory for Player endpoint integration tests.
///
/// Provisions:
/// - SQLite in-memory for the Master DB (users + tenant seeded)
/// - SQLite in-memory for the Tenant DB (Players schema created)
/// - <see cref="TestTenantDbContextFactory"/> that always resolves to the tenant SQLite DB
/// - Test authentication handler (always authenticates)
/// - No-op provisioning queue
/// </summary>
public sealed class PlayerWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>Pre-seeded user IDs — one per test that needs to create a player.</summary>
    public static readonly Guid[] TestUserIds =
    [
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000001"), // Post success
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000002"), // GetById success
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000003"), // Put success
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"), // Delete success
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000005"), // Post duplicate (used twice)
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000006"), // Player positions success
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000007"), // Player positions limit
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000008"), // Positions integration usage conflict
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000009"), // Player positions duplicate validation
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000010"), // Team integration player 1
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000011"), // Team integration player 2
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000012"), // Team integration player 3
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000013"), // Team integration player 4
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000014"), // Team integration player 5
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000015"), // Team integration player 6
    ];

    public const string TestUserEmail = "player-test@babaplay.com";
    public const string TestUserPassword = "PlayerTest@123456";
    public const string TestTenantSlug = "test-tenant-player";
    public static readonly Guid TestTenantId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-000000000001");

    private readonly SqliteConnection _masterConnection = new("Data Source=:memory:");
    private readonly SqliteConnection _tenantConnection = new("Data Source=:memory:");
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), $"babaplay-match-summary-tests-{Guid.NewGuid():N}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "player-integration-test-secret-key-exactly-256-bits-long-xxxxxxxxxxxxxxxxxx",
                ["Jwt:Issuer"] = "BabaPlay.Api",
                ["Jwt:Audience"] = "BabaPlay.Clients",
                ["Jwt:AccessTokenExpiresInMinutes"] = "60",
                ["Jwt:RefreshTokenExpiresInDays"] = "30",
                ["ConnectionStrings:MasterDb"] = "Data Source=:memory:",
                ["MatchSummaryStorage:RootPath"] = _storageRoot,
            });
        });

        builder.ConfigureServices(services =>
        {
            // --- Replace SQL Server MasterDbContext with SQLite ---
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
            foreach (var d in toRemove) services.Remove(d);

            _masterConnection.Open();
            services.AddDbContext<MasterDbContext>(o => o.UseSqlite(_masterConnection));

            // --- Replace TenantDbContextFactory with SQLite-backed test version ---
            var factoryDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(TenantDbContextFactory));
            if (factoryDescriptor is not null) services.Remove(factoryDescriptor);

            _tenantConnection.Open();
            services.AddScoped<TenantDbContextFactory>(_ =>
                new TestTenantDbContextFactory(_tenantConnection));

            // --- Replace provisioning queue with no-op ---
            var queueDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(ITenantProvisioningQueue));
            if (queueDescriptor is not null) services.Remove(queueDescriptor);
            services.AddSingleton<ITenantProvisioningQueue, NoOpProvisioningQueue>();

            // --- Replace JWT auth with test auth handler ---
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // --- Seed data ---
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

            if (Directory.Exists(_storageRoot))
                Directory.Delete(_storageRoot, recursive: true);
        }
    }

    private static async Task SeedMasterAsync(MasterDbContext db, UserManager<ApplicationUser> userManager)
    {
        for (int i = 0; i < TestUserIds.Length; i++)
        {
            var email = $"player-test-{i + 1}@babaplay.com";
            if (await userManager.FindByEmailAsync(email) is null)
            {
                var user = new ApplicationUser
                {
                    Id = TestUserIds[i].ToString(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    IsActive = true,
                };
                var result = await userManager.CreateAsync(user, TestUserPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        $"Seed user {i + 1} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Slug == TestTenantSlug);
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = TestTenantId,
                Name = "Test Tenant Player",
                Slug = TestTenantSlug,
                AssociationLatitude = -23.5505,
                AssociationLongitude = -46.6333,
                CheckinRadiusMeters = 300,
                IsActive = true,
                ProvisioningStatus = ProvisioningStatus.Ready,
                ConnectionString = "placeholder-overridden-by-test-factory",
            };

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        foreach (var testUserId in TestUserIds)
        {
            var userId = testUserId.ToString();
            var existsMembership = await db.UserTenants.AnyAsync(ut => ut.UserId == userId && ut.TenantId == tenant.Id);

            if (!existsMembership)
            {
                db.UserTenants.Add(new UserTenant
                {
                    UserId = userId,
                    TenantId = tenant.Id,
                    IsOwner = false,
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedTenantAsync(TenantDbContextFactory factory)
    {
        // Resolve tenantId from master DB is not straightforward here, so we
        // create a context directly using the factory's shared SQLite connection.
        // The TestTenantDbContextFactory ignores tenantId.
        await using var db = await factory.CreateAsync(Guid.Empty);
        await db.Database.EnsureCreatedAsync();

        var permissionByNormalized = await db.Permissions
            .ToDictionaryAsync(p => p.NormalizedCode, StringComparer.OrdinalIgnoreCase);

        foreach (var permissionCode in RbacCatalog.AllPermissions)
        {
            var normalizedCode = permissionCode.Trim().ToUpperInvariant();
            if (permissionByNormalized.ContainsKey(normalizedCode))
                continue;

            var permission = Permission.Create(permissionCode, $"Test permission: {permissionCode}");
            db.Permissions.Add(permission);
            permissionByNormalized[normalizedCode] = permission;
        }

        await db.SaveChangesAsync();

        var roleByNormalized = await db.Roles
            .Include(r => r.Permissions)
            .ToDictionaryAsync(r => r.NormalizedName, StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in RbacCatalog.DefaultRolePermissions.Keys)
        {
            var normalizedName = roleName.Trim().ToUpperInvariant();
            if (roleByNormalized.ContainsKey(normalizedName))
                continue;

            var role = Role.Create(TestTenantId, roleName, "Seeded test role");
            db.Roles.Add(role);
            roleByNormalized[normalizedName] = role;
        }

        await db.SaveChangesAsync();

        foreach (var roleEntry in RbacCatalog.DefaultRolePermissions)
        {
            var role = roleByNormalized[roleEntry.Key.Trim().ToUpperInvariant()];

            foreach (var permissionCode in roleEntry.Value)
            {
                var permission = permissionByNormalized[permissionCode.Trim().ToUpperInvariant()];
                role.AddPermission(permission.Id);
            }
        }

        await db.SaveChangesAsync();

        var adminRole = roleByNormalized[RbacCatalog.Roles.Admin.ToUpperInvariant()];
        foreach (var testUserId in TestUserIds)
        {
            var userId = testUserId.ToString();
            if (!await db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id))
                db.UserRoles.Add(UserRole.Create(userId, adminRole.Id));
        }

        await db.SaveChangesAsync();
    }

    // ---- Inner classes ----

    private sealed class TestTenantDbContextFactory : TenantDbContextFactory
    {
        private readonly SqliteConnection _connection;

        public TestTenantDbContextFactory(SqliteConnection connection)
            : base(null!) => _connection = connection;

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
