using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Interfaces;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Match>> GetAllActiveAsync(MatchStatus? status, CancellationToken ct = default);

    Task<bool> ExistsByGameDayAndTeamsAsync(
        Guid gameDayId,
        Guid homeTeamId,
        Guid awayTeamId,
        Guid? excludeMatchId,
        CancellationToken ct = default);

    Task AddAsync(Match match, CancellationToken ct = default);

    Task UpdateAsync(Match match, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}