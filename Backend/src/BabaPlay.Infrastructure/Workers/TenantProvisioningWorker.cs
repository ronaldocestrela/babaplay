using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Common;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BabaPlay.Infrastructure.Workers;

/// <summary>
/// Background worker that processes tenant provisioning jobs.
/// For each job: marks tenant as InProgress, creates an isolated SQL Server database,
/// runs EF Core migrations, then marks tenant as Ready (or Failed on error).
/// </summary>
public sealed class TenantProvisioningWorker : BackgroundService
{
    private readonly ITenantProvisioningQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TenantProvisioningWorker> _logger;

    public TenantProvisioningWorker(
        ITenantProvisioningQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<TenantProvisioningWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TenantProvisioningWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            Guid tenantId;
            try
            {
                tenantId = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await ProvisionAsync(tenantId, stoppingToken);
        }

        _logger.LogInformation("TenantProvisioningWorker stopped.");
    }

    private async Task ProvisionAsync(Guid tenantId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

        _logger.LogInformation("Provisioning tenant {TenantId}...", tenantId);

        try
        {
            await tenantRepo.UpdateProvisioningAsync(tenantId, ProvisioningStatus.InProgress, string.Empty, ct);

            var tenant = await masterDb.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);
            if (tenant is null)
            {
                _logger.LogWarning("Tenant {TenantId} not found; skipping provisioning.", tenantId);
                return;
            }

            var masterConnectionString = masterDb.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Master connection string is unavailable.");

            var dbName = BuildDatabaseName(tenantId);
            var tenantConnectionString = BuildConnectionString(masterConnectionString, dbName);

            // Create isolated database and apply EF migrations
            var tenantOptions = new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlServer(tenantConnectionString)
                .Options;

            await using var tenantCtx = new TenantDbContext(tenantOptions);
            await tenantCtx.Database.MigrateAsync(ct);
            await SeedDefaultRbacAsync(tenantCtx, tenantId, ct);
            await SeedOwnerAdminAssignmentsAsync(masterDb, tenantCtx, tenantId, ct);
            await SeedDefaultMatchEventTypesAsync(tenantCtx, tenantId, ct);

            await tenantRepo.UpdateProvisioningAsync(tenantId, ProvisioningStatus.Ready, tenantConnectionString, ct);
            _logger.LogInformation("Tenant {TenantId} provisioned successfully (db: {DbName}).", tenantId, dbName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision tenant {TenantId}.", tenantId);
            try
            {
                await tenantRepo.UpdateProvisioningAsync(tenantId, ProvisioningStatus.Failed, string.Empty, ct);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Failed to mark tenant {TenantId} as Failed.", tenantId);
            }
        }
    }

    /// <summary>
    /// Derives the database name from the tenant id using a deterministic format.
    /// Example: BabaPlay_Tenant_3f2a1b0c4d5e6f7a8b9c0d1e2f3a4b5c
    /// </summary>
    internal static string BuildDatabaseName(Guid tenantId)
        => $"BabaPlay_Tenant_{tenantId:N}";

    /// <summary>
    /// Replaces the <c>Database=...</c> segment in the master connection string
    /// to target the tenant-specific database.
    /// </summary>
    internal static string BuildConnectionString(string masterConnectionString, string dbName)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = dbName
        };
        return builder.ConnectionString;
    }

    private static async Task SeedDefaultRbacAsync(TenantDbContext tenantCtx, Guid tenantId, CancellationToken ct)
    {
        var permissionByNormalized = await tenantCtx.Permissions
            .ToDictionaryAsync(p => p.NormalizedCode, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var permissionCode in RbacCatalog.AllPermissions)
        {
            var normalizedCode = permissionCode.Trim().ToUpperInvariant();
            if (permissionByNormalized.ContainsKey(normalizedCode))
                continue;

            var permission = Permission.Create(permissionCode, $"Default permission: {permissionCode}");
            tenantCtx.Permissions.Add(permission);
            permissionByNormalized[normalizedCode] = permission;
        }

        await tenantCtx.SaveChangesAsync(ct);

        var roleByNormalized = await tenantCtx.Roles
            .Include(r => r.Permissions)
            .ToDictionaryAsync(r => r.NormalizedName, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var roleName in RbacCatalog.DefaultRolePermissions.Keys)
        {
            var normalizedName = roleName.Trim().ToUpperInvariant();
            if (roleByNormalized.ContainsKey(normalizedName))
                continue;

            var role = Role.Create(tenantId, roleName, "Seeded default role");
            tenantCtx.Roles.Add(role);
            roleByNormalized[normalizedName] = role;
        }

        await tenantCtx.SaveChangesAsync(ct);

        foreach (var roleEntry in RbacCatalog.DefaultRolePermissions)
        {
            var role = roleByNormalized[roleEntry.Key.Trim().ToUpperInvariant()];

            foreach (var permissionCode in roleEntry.Value)
            {
                var permission = permissionByNormalized[permissionCode.Trim().ToUpperInvariant()];
                role.AddPermission(permission.Id);
            }
        }

        await tenantCtx.SaveChangesAsync(ct);
    }

    private static async Task SeedDefaultMatchEventTypesAsync(TenantDbContext tenantCtx, Guid tenantId, CancellationToken ct)
    {
        var existingCodes = await tenantCtx.MatchEventTypes
            .Select(x => x.NormalizedCode)
            .ToListAsync(ct);

        var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);
        var defaults = new (string Code, string Name, int Points)[]
        {
            ("goal", "Goal", 2),
            ("yellow_card", "Yellow Card", -1),
            ("red_card", "Red Card", -3),
        };

        foreach (var item in defaults)
        {
            var normalized = item.Code.Trim().ToUpperInvariant();
            if (existing.Contains(normalized))
                continue;

            tenantCtx.MatchEventTypes.Add(MatchEventType.Create(
                tenantId,
                item.Code,
                item.Name,
                item.Points,
                isSystemDefault: true));
        }

        await tenantCtx.SaveChangesAsync(ct);
    }

    private static async Task SeedOwnerAdminAssignmentsAsync(
        MasterDbContext masterDb,
        TenantDbContext tenantCtx,
        Guid tenantId,
        CancellationToken ct)
    {
        var adminRoleNormalized = RbacCatalog.Roles.Admin.Trim().ToUpperInvariant();
        var adminRoleId = await tenantCtx.Roles
            .Where(r => r.NormalizedName == adminRoleNormalized)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);

        if (adminRoleId == Guid.Empty)
            return;

        var ownerUserIds = await masterDb.UserTenants
            .AsNoTracking()
            .Where(ut => ut.TenantId == tenantId && ut.IsOwner)
            .Select(ut => ut.UserId)
            .ToListAsync(ct);

        if (ownerUserIds.Count == 0)
            return;

        var existingAssignments = await tenantCtx.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == adminRoleId)
            .Select(ur => ur.UserId)
            .ToListAsync(ct);

        var existingSet = existingAssignments.ToHashSet(StringComparer.Ordinal);
        var hasChanges = false;

        foreach (var ownerUserId in ownerUserIds)
        {
            if (existingSet.Contains(ownerUserId))
                continue;

            tenantCtx.UserRoles.Add(UserRole.Create(ownerUserId, adminRoleId));
            hasChanges = true;
        }

        if (hasChanges)
            await tenantCtx.SaveChangesAsync(ct);
    }
}
