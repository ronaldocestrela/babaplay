using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetDefaultersListQueryHandler : IQueryHandler<GetDefaultersListQuery, Result<DefaultersListResponse>>
{
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetDefaultersListQueryHandler(
        IPlayerMonthlyFeeRepository monthlyFeeRepository,
        IPlayerRepository playerRepository)
    {
        _monthlyFeeRepository = monthlyFeeRepository;
        _playerRepository = playerRepository;
    }

    public async Task<Result<DefaultersListResponse>> HandleAsync(GetDefaultersListQuery query, CancellationToken ct = default)
    {
        var refDate = query.ReferenceUtc ?? DateTime.UtcNow;
        var overdueFees = await _monthlyFeeRepository.GetOverdueAsync(refDate, ct);

        if (overdueFees.Count == 0)
        {
            return Result<DefaultersListResponse>.Ok(new DefaultersListResponse(
                refDate,
                0,
                0m,
                0,
                Array.Empty<DefaulterMemberResponse>()));
        }

        var playerIds = overdueFees.Select(x => x.PlayerId).Distinct().ToList();
        var players = await _playerRepository.GetByIdsAsync(playerIds, ct);
        var playerMap = players.ToDictionary(p => p.Id, p => p.Name);

        var groups = overdueFees.GroupBy(x => x.PlayerId);
        var defaulterItems = new List<DefaulterMemberResponse>();

        foreach (var group in groups)
        {
            var playerId = group.Key;
            var playerName = playerMap.TryGetValue(playerId, out var name) ? name : "Atleta";
            var count = group.Count();
            var totalOverdue = group.Sum(x => x.Amount - x.PaidAmount);
            var maxDaysOverdue = group.Max(x => Math.Max(0, (int)(refDate.Date - x.DueDateUtc.Date).TotalDays));
            var competences = group.Select(x => $"{x.Month:D2}/{x.Year}").Distinct().ToList();

            defaulterItems.Add(new DefaulterMemberResponse(
                playerId,
                playerName,
                count,
                totalOverdue,
                maxDaysOverdue,
                competences,
                null));
        }

        var orderedItems = defaulterItems
            .OrderByDescending(x => x.MaxDaysOverdue)
            .ThenByDescending(x => x.TotalOverdueAmount)
            .ToList();

        var totalDefaulters = orderedItems.Count;
        var grandTotalOverdue = orderedItems.Sum(x => x.TotalOverdueAmount);
        var avgDays = totalDefaulters > 0 ? orderedItems.Average(x => x.MaxDaysOverdue) : 0;

        return Result<DefaultersListResponse>.Ok(new DefaultersListResponse(
            refDate,
            totalDefaulters,
            grandTotalOverdue,
            avgDays,
            orderedItems));
    }
}
