using BabaPlay.Modules.Associates.Entities;
using BabaPlay.Modules.Identity;
using BabaPlay.Modules.Identity.Entities;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class TenantDatabaseProvisioner : ITenantProvisioningService
{
    private readonly ILogger<TenantDatabaseProvisioner> _logger;

    public TenantDatabaseProvisioner(ILogger<TenantDatabaseProvisioner> logger) => _logger = logger;

    public Task<Result> ProvisionDatabaseAsync(string databaseName, string platformConnectionString, CancellationToken cancellationToken = default) =>
        ProvisionAsync(databaseName, platformConnectionString, cancellationToken);

    public async Task<Result> ProvisionAsync(string databaseName, string platformConnectionString, CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseExistsAsync(platformConnectionString, databaseName, cancellationToken);

        var tenantCs = BuildConnectionString(platformConnectionString, databaseName);
        var options = new DbContextOptionsBuilder<TenantDbContext>().UseSqlServer(tenantCs).Options;
        await using var ctx = new TenantDbContext(options);
        await ctx.Database.MigrateAsync(cancellationToken);
        await SeedTenantDefaultsAsync(ctx, cancellationToken);
        _logger.LogInformation("Tenant database ready: {Db}", databaseName);
        return SharedKernel.Results.Result.Success();
    }

    private static async Task EnsureDatabaseExistsAsync(string platformConnectionString, string databaseName, CancellationToken ct)
    {
        var master = BuildConnectionString(platformConnectionString, "master");
        await using var conn = new SqlConnection(master);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT DB_ID(@name)";
        cmd.Parameters.AddWithValue("@name", databaseName.Trim());
        var id = await cmd.ExecuteScalarAsync(ct);
        if (id != null && id != DBNull.Value) return;

        await using var create = conn.CreateCommand();
        create.CommandText = $"CREATE DATABASE [{EscapeDatabaseName(databaseName)}]";
        await create.ExecuteNonQueryAsync(ct);
    }

    private static string EscapeDatabaseName(string databaseName) =>
        databaseName.Replace("]", "]]", StringComparison.Ordinal);

    private static string BuildConnectionString(string platformConnectionString, string databaseName)
    {
        var sb = new SqlConnectionStringBuilder(platformConnectionString)
        {
            InitialCatalog = databaseName
        };
        return sb.ConnectionString;
    }

    private static async Task SeedTenantDefaultsAsync(TenantDbContext ctx, CancellationToken ct)
    {
        if (!await ctx.Permissions.AnyAsync(ct))
        {
            foreach (var name in DefaultPermissionNames)
            {
                await ctx.Permissions.AddAsync(new Permission { Name = name }, ct);
            }

            await ctx.SaveChangesAsync(ct);
        }

        if (!await ctx.Roles.AnyAsync(ct))
        {
            var roles = new[]
            {
                new ApplicationRole
                {
                    Id = Guid.CreateVersion7().ToString("N"),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.CreateVersion7().ToString("N")
                },
                new ApplicationRole
                {
                    Id = Guid.CreateVersion7().ToString("N"),
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = Guid.CreateVersion7().ToString("N")
                },
                new ApplicationRole
                {
                    Id = Guid.CreateVersion7().ToString("N"),
                    Name = "Associate",
                    NormalizedName = "ASSOCIATE",
                    ConcurrencyStamp = Guid.CreateVersion7().ToString("N")
                }
            };

            await ctx.Roles.AddRangeAsync(roles, ct);
            await ctx.SaveChangesAsync(ct);

            var perms = await ctx.Permissions.AsNoTracking().ToListAsync(ct);
            var byName = perms.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            string RoleId(string n) => roles.First(r => r.Name == n).Id;

            void Grant(string role, IEnumerable<string> permNames)
            {
                foreach (var pn in permNames)
                {
                    if (!byName.TryGetValue(pn, out var p)) continue;
                    ctx.RolePermissions.Add(new RolePermission { RoleId = RoleId(role), PermissionId = p.Id });
                }
            }

            Grant("Admin", DefaultPermissionNames);
            Grant("Manager", DefaultPermissionNames.Where(x => x != "users.manage"));
            Grant("Associate", new[] { "associates.read", "checkins.manage" });

            await ctx.SaveChangesAsync(ct);
        }

        if (!await ctx.Positions.AnyAsync(ct))
        {
            var positions = new[]
            {
                ("Goleiro", 1), ("Zagueiro", 2), ("Lateral", 3),
                ("Meia", 4), ("Atacante", 5)
            };
            foreach (var (name, order) in positions)
            {
                await ctx.Positions.AddAsync(new Position { Name = name, SortOrder = order }, ct);
            }

            await ctx.SaveChangesAsync(ct);
        }
    }

    private static readonly string[] DefaultPermissionNames =
    [
        "associates.read", "associates.manage",
        "checkins.manage", "teams.generate",
        "financial.read", "financial.manage",
        "association.manage", "users.manage"
    ];
}
