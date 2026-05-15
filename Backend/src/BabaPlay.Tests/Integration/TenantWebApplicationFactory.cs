using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using BabaPlay.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// WebApplicationFactory for multi-tenancy integration tests.
/// Uses SQLite in-memory for the Master DB and replaces the provisioning queue
/// with a no-op implementation so the background worker never tries to create
/// real SQL Server databases.
/// </summary>
public sealed class TenantWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestUserEmail = "tenant-integration@babaplay.com";
    public const string TestUserPassword = "Integration@123456";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "integration-test-secret-key-exactly-256-bits-long-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                ["Jwt:Issuer"] = "BabaPlay.Api",
                ["Jwt:Audience"] = "BabaPlay.Clients",
                ["Jwt:AccessTokenExpiresInMinutes"] = "60",
                ["Jwt:RefreshTokenExpiresInDays"] = "30",
                ["ConnectionStrings:MasterDb"] = "Data Source=:memory:",
                ["TenantLogoStorage:Provider"] = "Cloudinary",
                ["TenantLogoStorage:Cloudinary:CloudName"] = "integration-cloud",
                ["TenantLogoStorage:Cloudinary:ApiKey"] = "integration-api-key",
                ["TenantLogoStorage:Cloudinary:ApiSecret"] = "integration-api-secret",
                ["TenantLogoStorage:Cloudinary:Folder"] = "tenant-logos",
            });
        });

        builder.ConfigureServices(services =>
        {
            // --- Replace SQL Server with SQLite ---
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

            _connection.Open();
            services.AddDbContext<MasterDbContext>(o => o.UseSqlite(_connection));

            // --- Replace provisioning queue with no-op ---
            var queueDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITenantProvisioningQueue));
            if (queueDescriptor is not null) services.Remove(queueDescriptor);
            services.AddSingleton<ITenantProvisioningQueue, NoOpProvisioningQueue>();

            // --- Replace cloudinary uploader with deterministic fake ---
            services.RemoveAll<ICloudinaryImageUploader>();
            services.AddScoped<ICloudinaryImageUploader, FakeCloudinaryImageUploader>();

            // --- Ensure tenant logo storage returns remote URL in integration tests ---
            services.RemoveAll<ITenantLogoStorageService>();
            services.AddScoped<ITenantLogoStorageService, FakeTenantLogoStorageService>();

            // --- Replace JWT auth with test auth handler (always authenticates) ---
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // --- Seed test data ---
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
        if (await userManager.FindByIdAsync(TestAuthHandler.TestUserId) is not null)
            return;

        var user = new ApplicationUser
        {
            Id = TestAuthHandler.TestUserId,
            UserName = TestAuthHandler.TestUserEmail,
            Email = TestAuthHandler.TestUserEmail,
            EmailConfirmed = true,
            IsActive = true,
        };
        var result = await userManager.CreateAsync(user, TestUserPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Test seed failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// No-op queue: discards enqueue calls; DequeueAsync blocks until cancellation.
    /// Prevents the BackgroundService worker from attempting real DB provisioning.
    /// </summary>
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

    private sealed class FakeCloudinaryImageUploader : ICloudinaryImageUploader
    {
        public Task<CloudinaryImageUploadResult> UploadAsync(CloudinaryImageUploadRequest request, CancellationToken ct = default)
        {
            var safeFolder = request.Folder.Replace("//", "/");
            var safePublicId = request.PublicId.Trim('/');
            var url = $"https://res.cloudinary.com/integration-cloud/image/upload/v1/{safeFolder}/{safePublicId}";

            return Task.FromResult(new CloudinaryImageUploadResult(
                true,
                url,
                $"{safeFolder}/{safePublicId}",
                request.Content.LongLength,
                null));
        }
    }

    private sealed class FakeTenantLogoStorageService : ITenantLogoStorageService
    {
        public Task<TenantLogoStoredFile> SaveAsync(TenantLogoSaveRequest request, CancellationToken ct = default)
        {
            var extension = Path.GetExtension(request.FileName);
            var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".bin" : extension.ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{safeExtension}";
            var url = $"https://res.cloudinary.com/integration-cloud/image/upload/v1/tenant-logos/{request.TenantId:N}/{fileName}";

            return Task.FromResult(new TenantLogoStoredFile(
                url,
                request.ContentType,
                request.Content.LongLength));
        }
    }
}
