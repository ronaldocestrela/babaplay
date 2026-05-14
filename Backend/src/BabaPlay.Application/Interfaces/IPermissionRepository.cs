using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Persistence abstraction for permission data stored in tenant database.
/// </summary>
public interface IPermissionRepository
{
    Task<Permission?> GetByNormalizedCodeAsync(string normalizedCode, CancellationToken ct = default);
    Task AddAsync(Permission permission, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
