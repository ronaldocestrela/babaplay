using BabaPlay.Application.Common;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Keeps tenant-scoped RBAC permissions and default roles aligned with <see cref="RbacCatalog"/>.
/// </summary>
public interface ITenantRbacCatalogSyncService
{
    /// <summary>
    /// Ensures catalog permissions, default roles, and role-permission grants exist for one tenant.
    /// Idempotent: safe to run on every startup.
    /// </summary>
    Task<Result> SyncTenantAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Runs <see cref="SyncTenantAsync"/> for every active tenant.
    /// </summary>
    Task<Result> SyncAllTenantsAsync(CancellationToken ct = default);
}
