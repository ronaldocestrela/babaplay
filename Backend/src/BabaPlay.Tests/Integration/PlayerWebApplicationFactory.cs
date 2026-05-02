using BabaPlay.Application.Interfaces;
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
    ];

    public const string TestUserEmail = "player-test@babaplay.com";
    public const string TestUserPassword = "PlayerTest@123456";
    public const string TestTenantSlug = "test-tenant-player";

    private readonly SqliteConnection _masterConnection = new("Data Source=:memory:");
    private readonly SqliteConnection _tenantConnection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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

        if (!db.Tenants.Any(t => t.Slug == TestTenantSlug))
        {
            db.Tenants.Add(new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Test Tenant Player",
                Slug = TestTenantSlug,
                IsActive = true,
                ProvisioningStatus = ProvisioningStatus.Ready,
                ConnectionString = "placeholder-overridden-by-test-factory",
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedTenantAsync(TenantDbContextFactory factory)
    {
        // Resolve tenantId from master DB is not straightforward here, so we
        // create a context directly using the factory's shared SQLite connection.
        // The TestTenantDbContextFactory ignores tenantId.
        var db = await factory.CreateAsync(Guid.Empty);
        await db.Database.EnsureCreatedAsync();
        await db.DisposeAsync();
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
