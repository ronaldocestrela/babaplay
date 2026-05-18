using BabaPlay.Application.Common;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Bootstraps tenant-scoped RBAC defaults and grants Admin access to the tenant owner.
/// </summary>
public interface ITenantOwnerRbacBootstrapService
{
    /// <summary>
    /// Ensures default permissions/roles exist for the tenant and assigns Admin role to owner user.
    /// </summary>
    Task<Result> EnsureOwnerAdminAccessAsync(string userId, Guid tenantId, CancellationToken ct = default);
}