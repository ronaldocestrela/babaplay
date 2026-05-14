using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetCashFlowQueryHandler : IQueryHandler<GetCashFlowQuery, Result<CashFlowResponse>>
{
    private readonly ICashTransactionRepository _cashTransactionRepository;

    public GetCashFlowQueryHandler(ICashTransactionRepository cashTransactionRepository)
        => _cashTransactionRepository = cashTransactionRepository;

    public async Task<Result<CashFlowResponse>> HandleAsync(GetCashFlowQuery query, CancellationToken ct = default)
    {
        if (query.FromUtc.Kind != DateTimeKind.Utc || query.ToUtc.Kind != DateTimeKind.Utc || query.FromUtc > query.ToUtc)
            return Result<CashFlowResponse>.Fail("INVALID_PERIOD", "FromUtc and ToUtc must be UTC and FromUtc <= ToUtc.");

        var transactions = await _cashTransactionRepository.GetByPeriodAsync(query.FromUtc, query.ToUtc, ct);

        var totalIncome = transactions
            .Where(x => x.Type == CashTransactionType.Income)
            .Sum(x => x.Amount);

        var totalExpense = transactions
            .Where(x => x.Type == CashTransactionType.Expense)
            .Sum(x => x.Amount);

        var entries = transactions
            .Select(x => new CashFlowEntryResponse(
                x.Id,
                x.PlayerId,
                x.Type,
                x.Amount,
                x.SignedAmount,
                x.OccurredOnUtc,
                x.Description))
            .ToList();

        return Result<CashFlowResponse>.Ok(new CashFlowResponse(
            query.FromUtc,
            query.ToUtc,
            totalIncome,
            totalExpense,
            totalIncome - totalExpense,
            entries));
    }
}
