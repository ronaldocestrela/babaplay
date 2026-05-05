using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that replaces SQL Server with SQLite in-memory for integration tests.
/// A single SqliteConnection is kept alive so the in-memory database persists across all requests.
/// </summary>
public sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    // Known test credentials used across integration tests
    public const string TestUserEmail = "integration@babaplay.com";
    public const string TestUserPassword = "Integration@123456";
    public const string ValidRefreshToken = "valid-test-refresh-token-phase1-integration";
    public const string ExpiredRefreshToken = "expired-test-refresh-token-phase1-integration";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // 1. Inject test-specific configuration (JWT + SQLite placeholder)
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "integration-test-secret-key-exactly-256-bits-long-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                ["Jwt:Issuer"] = "BabaPlay.Api",
                ["Jwt:Audience"] = "BabaPlay.Clients",
                ["Jwt:AccessTokenExpiresInMinutes"] = "60",
                ["Jwt:RefreshTokenExpiresInDays"] = "30",
                // placeholder — DbContext will be replaced below
                ["ConnectionStrings:MasterDb"] = "Data Source=:memory:",
            });
        });

        // 2. Replace SQL Server DbContext with SQLite sharing a single connection
        builder.ConfigureServices(services =>
        {
            // Remove all services registered by the SQL Server DbContext setup.
            // EF Core registers: DbContextOptions<T>, IDbContextOptionsConfiguration<T>,
            // plus internal provider-specific singletons whose types are defined in the
            // SqlServer assembly. We target all three removal vectors.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<MasterDbContext>) ||
                    d.ServiceType == typeof(MasterDbContext) ||
                    // IDbContextOptionsConfiguration<MasterDbContext>
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().FullName ==
                         "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration`1" &&
                     d.ServiceType.GenericTypeArguments.FirstOrDefault() == typeof(MasterDbContext)) ||
                    // Any service whose concrete type lives in the SqlServer EF assembly
                    (d.ImplementationType?.Assembly.GetName().Name?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true) ||
                    (d.ImplementationInstance?.GetType().Assembly.GetName().Name?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            _connection.Open();
            services.AddDbContext<MasterDbContext>(options => options.UseSqlite(_connection));

            // Seed the in-memory DB once, using a temporary scope
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            db.Database.EnsureCreated();
            SeedAsync(db, userManager).GetAwaiter().GetResult();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }

    private static async Task SeedAsync(MasterDbContext db, UserManager<ApplicationUser> userManager)
    {
        var createdUser = await userManager.FindByEmailAsync(TestUserEmail);

        if (createdUser is null)
        {
            var user = new ApplicationUser
            {
                UserName = TestUserEmail,
                Email = TestUserEmail,
                EmailConfirmed = true,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(user, TestUserPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Test seed failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            createdUser = await userManager.FindByEmailAsync(TestUserEmail)
                ?? throw new InvalidOperationException("Seeded user not found.");
        }

        var hasValidToken = await db.RefreshTokens.AnyAsync(t => t.Token == ValidRefreshToken);
        if (!hasValidToken)
        {
            db.RefreshTokens.Add(new RefreshToken
            {
                Token = ValidRefreshToken,
                UserId = createdUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
            });
        }

        var hasExpiredToken = await db.RefreshTokens.AnyAsync(t => t.Token == ExpiredRefreshToken);
        if (!hasExpiredToken)
        {
            db.RefreshTokens.Add(new RefreshToken
            {
                Token = ExpiredRefreshToken,
                UserId = createdUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-31),
            });
        }

        await db.SaveChangesAsync();
    }
}
