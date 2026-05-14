using BabaPlay.Application.Common;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Handles bootstrap of the initial tenant owner in the master identity database.
/// </summary>
public interface ITenantOwnerProvisioningService
{
    /// <summary>
    /// Resolves the owner user id either from the authenticated requester or from
    /// explicit admin credentials supplied during tenant onboarding.
    /// </summary>
    Task<Result<string>> ResolveOwnerUserIdAsync(
        string? requestedByUserId,
        string? adminEmail,
        string? adminPassword,
        CancellationToken ct = default);

    /// <summary>
    /// Ensures the owner user is linked to the tenant in master DB.
    /// </summary>
    Task<Result> EnsureOwnerMembershipAsync(string userId, Guid tenantId, CancellationToken ct = default);
}
