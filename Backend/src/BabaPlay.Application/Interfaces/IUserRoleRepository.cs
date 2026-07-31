using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Persistence abstraction for role assignments to users in tenant scope.
/// </summary>
public interface IUserRoleRepository
{
    Task<bool> ExistsAsync(string userId, Guid roleId, CancellationToken ct = default);
    Task AddAsync(UserRole userRole, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(string userId, string normalizedPermissionCode, CancellationToken ct = default);

    /// <summary>Returns distinct permission codes granted to the user through active roles in the current tenant.</summary>
    Task<IReadOnlyList<string>> GetPermissionCodesAsync(string userId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
