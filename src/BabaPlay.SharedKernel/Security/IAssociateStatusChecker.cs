namespace BabaPlay.SharedKernel.Security;

/// <summary>
/// Tenant-scoped check: whether the user may sign in as an associate.
/// Returns true when no associate row is linked to the user (e.g. staff) or when the associate is active.
/// </summary>
public interface IAssociateStatusChecker
{
    Task<bool> IsActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
