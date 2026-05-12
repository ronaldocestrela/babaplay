namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Read-only service used by onboarding flows that need to know whether
/// a user already has an active player profile in a tenant.
/// </summary>
public interface IPlayerOnboardingReadService
{
    /// <summary>
    /// Returns true when the user has an active player profile in the tenant.
    /// </summary>
    Task<bool> HasActivePlayerProfileAsync(Guid tenantId, string userId, CancellationToken ct = default);
}
