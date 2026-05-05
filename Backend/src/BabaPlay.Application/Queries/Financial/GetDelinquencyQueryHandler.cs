using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetDelinquencyQueryHandler : IQueryHandler<GetDelinquencyQuery, Result<DelinquencyResponse>>
{
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;

    public GetDelinquencyQueryHandler(IPlayerMonthlyFeeRepository monthlyFeeRepository)
        => _monthlyFeeRepository = monthlyFeeRepository;

    public async Task<Result<DelinquencyResponse>> HandleAsync(GetDelinquencyQuery query, CancellationToken ct = default)
    {
        if (query.ReferenceUtc.Kind != DateTimeKind.Utc)
            return Result<DelinquencyResponse>.Fail("INVALID_PERIOD", "ReferenceUtc must be UTC.");

        var overdueFees = await _monthlyFeeRepository.GetOverdueAsync(query.ReferenceUtc, ct);

        var items = overdueFees.Select(x =>
        {
            var openAmount = x.Amount - x.PaidAmount;
            var daysOverdue = Math.Max(0, (int)(query.ReferenceUtc.Date - x.DueDateUtc.Date).TotalDays);

            return new DelinquencyEntryResponse(
                x.Id,
                x.PlayerId,
                x.Year,
                x.Month,
                x.Amount,
                x.PaidAmount,
                openAmount,
                x.DueDateUtc,
                daysOverdue);
        }).ToList();

        return Result<DelinquencyResponse>.Ok(new DelinquencyResponse(
            query.ReferenceUtc,
            items.Sum(x => x.OpenAmount),
            items));
    }
}
