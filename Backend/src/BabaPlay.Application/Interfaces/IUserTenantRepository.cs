using BabaPlay.Application.DTOs;

namespace BabaPlay.Application.Interfaces;

/// <summary>Validates membership between users and tenants in the Master database.</summary>
public interface IUserTenantRepository
{
    /// <summary>Returns true when the user belongs to the given tenant.</summary>
    Task<bool> IsMemberAsync(string userId, Guid tenantId, CancellationToken ct = default);

    /// <summary>Returns all active tenant memberships for the user.</summary>
    Task<IReadOnlyList<AuthTenantMembershipDto>> GetMembershipsAsync(string userId, CancellationToken ct = default);
}
