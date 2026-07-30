using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetFinancialOverviewQueryHandler : IQueryHandler<GetFinancialOverviewQuery, Result<FinancialOverviewResponse>>
{
    private readonly ICashTransactionRepository _cashTransactionRepository;
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;

    public GetFinancialOverviewQueryHandler(
        ICashTransactionRepository cashTransactionRepository,
        IPlayerMonthlyFeeRepository monthlyFeeRepository)
    {
        _cashTransactionRepository = cashTransactionRepository;
        _monthlyFeeRepository = monthlyFeeRepository;
    }

    public async Task<Result<FinancialOverviewResponse>> HandleAsync(GetFinancialOverviewQuery query, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var year = query.Year ?? now.Year;
        var month = query.Month ?? now.Month;

        if (year < 1 || year > 9999)
            return Result<FinancialOverviewResponse>.Fail("INVALID_COMPETENCE", "Year must be between 1 and 9999.");

        if (month < 1 || month > 12)
            return Result<FinancialOverviewResponse>.Fail("INVALID_COMPETENCE", "Month must be between 1 and 12.");

        var fromUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = fromUtc.AddMonths(1).AddTicks(-1);

        var cashTransactions = await _cashTransactionRepository.GetByPeriodAsync(fromUtc, toUtc, ct);
        var monthlyFees = await _monthlyFeeRepository.GetByCompetenceAsync(year, month, ct);
        var overdueFees = await _monthlyFeeRepository.GetOverdueAsync(now, ct);

        var totalIncome = cashTransactions.Where(x => x.Type == CashTransactionType.Income).Sum(x => x.Amount);
        var totalExpense = cashTransactions.Where(x => x.Type == CashTransactionType.Expense).Sum(x => x.Amount);
        var netBalance = totalIncome - totalExpense;

        var pendingMonthlyFeesAmount = overdueFees.Sum(x => x.Amount - x.PaidAmount);
        var delinquentPlayersCount = overdueFees.Select(x => x.PlayerId).Distinct().Count();

        var totalMonthlyFees = monthlyFees.Sum(x => x.Amount);
        var paidMonthlyFees = monthlyFees.Sum(x => x.PaidAmount);
        var collectionRatePercentage = totalMonthlyFees > 0
            ? Math.Round((paidMonthlyFees / totalMonthlyFees) * 100m, 2)
            : 100m;

        var recentTransactions = cashTransactions
            .OrderByDescending(x => x.OccurredOnUtc)
            .Take(10)
            .Select(x => new CashTransactionResponse(
                x.Id,
                x.TenantId,
                x.PlayerId,
                x.Type,
                x.Amount,
                x.SignedAmount,
                x.OccurredOnUtc,
                x.Description,
                x.IsActive,
                x.CreatedAt))
            .ToList();

        return Result<FinancialOverviewResponse>.Ok(new FinancialOverviewResponse(
            year,
            month,
            totalIncome,
            totalExpense,
            netBalance,
            pendingMonthlyFeesAmount,
            delinquentPlayersCount,
            collectionRatePercentage,
            recentTransactions));
    }
}
