using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetPlayerStatementQueryHandler : IQueryHandler<GetPlayerStatementQuery, Result<PlayerStatementResponse>>
{
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;

    public GetPlayerStatementQueryHandler(IPlayerMonthlyFeeRepository monthlyFeeRepository)
        => _monthlyFeeRepository = monthlyFeeRepository;

    public async Task<Result<PlayerStatementResponse>> HandleAsync(GetPlayerStatementQuery query, CancellationToken ct = default)
    {
        if (query.PlayerId == Guid.Empty)
            return Result<PlayerStatementResponse>.Fail("FINANCIAL_INVALID_PLAYER_ID", "PlayerId is required.");

        if (query.FromUtc.Kind != DateTimeKind.Utc || query.ToUtc.Kind != DateTimeKind.Utc || query.FromUtc > query.ToUtc)
            return Result<PlayerStatementResponse>.Fail("INVALID_PERIOD", "FromUtc and ToUtc must be UTC and FromUtc <= ToUtc.");

        var monthlyFees = await _monthlyFeeRepository.GetByPlayerAndPeriodAsync(query.PlayerId, query.FromUtc, query.ToUtc, ct);

        var items = monthlyFees
            .Select(x => new PlayerStatementEntryResponse(
                x.Id,
                x.Year,
                x.Month,
                x.Amount,
                x.PaidAmount,
                x.DueDateUtc,
                x.PaidAtUtc,
                x.Status,
                x.Amount - x.PaidAmount))
            .ToList();

        return Result<PlayerStatementResponse>.Ok(new PlayerStatementResponse(
            query.PlayerId,
            query.FromUtc,
            query.ToUtc,
            items.Sum(x => x.Amount),
            items.Sum(x => x.PaidAmount),
            items.Sum(x => x.OpenAmount),
            items));
    }
}
