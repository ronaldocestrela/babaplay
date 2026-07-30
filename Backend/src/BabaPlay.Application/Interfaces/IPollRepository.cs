using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Repository contract for the Poll aggregate (F25 — Interactive Polls).
/// </summary>
public interface IPollRepository
{
    /// <summary>Returns all active polls for a tenant, newest first.</summary>
    Task<IReadOnlyList<Poll>> GetByTenantAsync(
        Guid tenantId,
        bool onlyActive = true,
        CancellationToken ct = default);

    /// <summary>Returns a single poll by id with its options and votes, validated against the tenant.</summary>
    Task<Poll?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);

    /// <summary>Persists a new poll and saves immediately.</summary>
    Task AddAsync(Poll poll, CancellationToken ct = default);

    /// <summary>Updates an existing poll and saves immediately.</summary>
    Task UpdateAsync(Poll poll, CancellationToken ct = default);

    /// <summary>Records a user vote for a poll option.</summary>
    Task AddVoteAsync(PollVote vote, CancellationToken ct = default);

    /// <summary>Returns true if the given user has already voted in the given poll.</summary>
    Task<bool> HasUserVotedAsync(Guid pollId, string userId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
