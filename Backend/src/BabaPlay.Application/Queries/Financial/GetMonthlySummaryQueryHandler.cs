using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetMonthlySummaryQueryHandler : IQueryHandler<GetMonthlySummaryQuery, Result<MonthlySummaryResponse>>
{
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;
    private readonly ICashTransactionRepository _cashTransactionRepository;

    public GetMonthlySummaryQueryHandler(
        IPlayerMonthlyFeeRepository monthlyFeeRepository,
        ICashTransactionRepository cashTransactionRepository)
    {
        _monthlyFeeRepository = monthlyFeeRepository;
        _cashTransactionRepository = cashTransactionRepository;
    }

    public async Task<Result<MonthlySummaryResponse>> HandleAsync(GetMonthlySummaryQuery query, CancellationToken ct = default)
    {
        if (query.Month < 1 || query.Month > 12)
            return Result<MonthlySummaryResponse>.Fail("INVALID_COMPETENCE", "Month must be between 1 and 12.");

        var monthlyFees = await _monthlyFeeRepository.GetByCompetenceAsync(query.Year, query.Month, ct);

        var fromUtc = new DateTime(query.Year, query.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = fromUtc.AddMonths(1).AddTicks(-1);
        var cashTransactions = await _cashTransactionRepository.GetByPeriodAsync(fromUtc, toUtc, ct);

        var monthlyFeesAmount = monthlyFees.Sum(x => x.Amount);
        var monthlyFeesPaidAmount = monthlyFees.Sum(x => x.PaidAmount);
        var cashIncome = cashTransactions.Where(x => x.Type == CashTransactionType.Income).Sum(x => x.Amount);
        var cashExpense = cashTransactions.Where(x => x.Type == CashTransactionType.Expense).Sum(x => x.Amount);

        return Result<MonthlySummaryResponse>.Ok(new MonthlySummaryResponse(
            query.Year,
            query.Month,
            monthlyFeesAmount,
            monthlyFeesPaidAmount,
            monthlyFeesAmount - monthlyFeesPaidAmount,
            cashIncome,
            cashExpense,
            cashIncome - cashExpense));
    }
}
