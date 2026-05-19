using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Common;
using BabaPlay.Domain.Entities;
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
/// - SQLite in-memory for the unified App DB (users + tenant data seeded)
/// - Test authentication handler (always authenticates)
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
    public const string TestTenantBSlug = "test-tenant-player-b";
    public static readonly Guid TestTenantBId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-000000000002");

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
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
            // --- Replace SQL Server AppDbContext with SQLite ---
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().FullName ==
                         "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration`1" &&
                     d.ServiceType.GenericTypeArguments.FirstOrDefault() == typeof(AppDbContext)) ||
                    (d.ImplementationType?.Assembly.GetName().Name?
                        .Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true) ||
                    (d.ImplementationInstance?.GetType().Assembly.GetName().Name?
                        .Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            _connection.Open();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));

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

            var masterDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            masterDb.Database.EnsureCreated();
            SeedMasterAsync(masterDb, userManager).GetAwaiter().GetResult();

            SeedTenantAsync(masterDb).GetAwaiter().GetResult();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();

            if (Directory.Exists(_storageRoot))
                Directory.Delete(_storageRoot, recursive: true);
        }
    }

    private static async Task SeedMasterAsync(AppDbContext db, UserManager<ApplicationUser> userManager)
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
            };

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        var tenantB = await db.Tenants.FirstOrDefaultAsync(t => t.Slug == TestTenantBSlug);
        if (tenantB is null)
        {
            tenantB = new Tenant
            {
                Id = TestTenantBId,
                Name = "Test Tenant Player B",
                Slug = TestTenantBSlug,
                AssociationLatitude = -23.5505,
                AssociationLongitude = -46.6333,
                CheckinRadiusMeters = 300,
                IsActive = true,
            };

            db.Tenants.Add(tenantB);
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
                    IsOwner = testUserId == TestUserIds[0],
                });
            }
        }

        var firstUserId = TestUserIds[0].ToString();
        var hasMembershipInTenantB = await db.UserTenants.AnyAsync(
            ut => ut.UserId == firstUserId && ut.TenantId == tenantB.Id);

        if (!hasMembershipInTenantB)
        {
            db.UserTenants.Add(new UserTenant
            {
                UserId = firstUserId,
                TenantId = tenantB.Id,
                IsOwner = true,
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedTenantAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        var permissionByNormalized = await db.Permissions
            .ToDictionaryAsync(p => p.NormalizedCode, StringComparer.OrdinalIgnoreCase);

        foreach (var permissionCode in RbacCatalog.AllPermissions)
        {
            var normalizedCode = permissionCode.Trim().ToUpperInvariant();
            if (permissionByNormalized.ContainsKey(normalizedCode))
                continue;

            var permission = Permission.Create(TestTenantId, permissionCode, $"Test permission: {permissionCode}");
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

}
