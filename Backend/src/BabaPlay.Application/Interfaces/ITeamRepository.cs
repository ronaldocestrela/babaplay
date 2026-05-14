using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Persistence abstraction for tenant-scoped teams.
/// </summary>
public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Team>> GetAllActiveAsync(CancellationToken ct = default);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken ct = default);

    Task AddAsync(Team team, CancellationToken ct = default);

    Task UpdateAsync(Team team, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
