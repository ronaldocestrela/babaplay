namespace BabaPlay.Application.Interfaces;

public interface IUserTenantMembershipService
{
    Task<bool> EnsureMemberAsync(string userId, Guid tenantId, CancellationToken ct = default);
}
