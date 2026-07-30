using System.Globalization;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetFinancialStatementQueryHandler : IQueryHandler<GetFinancialStatementQuery, Result<FinancialStatementResponse>>
{
    private readonly ICashTransactionRepository _cashTransactionRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetFinancialStatementQueryHandler(
        ICashTransactionRepository cashTransactionRepository,
        IPlayerRepository playerRepository)
    {
        _cashTransactionRepository = cashTransactionRepository;
        _playerRepository = playerRepository;
    }

    public async Task<Result<FinancialStatementResponse>> HandleAsync(GetFinancialStatementQuery query, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var year = query.Year ?? now.Year;
        var month = query.Month ?? now.Month;

        if (month < 1 || month > 12)
            return Result<FinancialStatementResponse>.Fail("INVALID_MONTH", "Month must be between 1 and 12.");

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var fromUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(year, month, daysInMonth, 23, 59, 59, DateTimeKind.Utc);

        var transactions = await _cashTransactionRepository.GetByPeriodAsync(fromUtc, toUtc, ct);

        var playerIds = transactions
            .Where(x => x.PlayerId.HasValue && x.PlayerId.Value != Guid.Empty)
            .Select(x => x.PlayerId!.Value)
            .Distinct()
            .ToList();

        var players = playerIds.Count > 0 ? await _playerRepository.GetByIdsAsync(playerIds, ct) : Array.Empty<BabaPlay.Domain.Entities.Player>();
        var playerMap = players.ToDictionary(p => p.Id, p => p.Name);

        var items = transactions.Select(tx =>
        {
            var playerName = tx.PlayerId.HasValue && playerMap.TryGetValue(tx.PlayerId.Value, out var name)
                ? name
                : null;

            var category = CategorizeTransaction(tx.Description, tx.Type);

            return new FinancialStatementItemResponse(
                tx.Id,
                tx.OccurredOnUtc,
                tx.Description,
                tx.Type,
                tx.Amount,
                category,
                tx.PlayerId,
                playerName);
        }).OrderByDescending(x => x.OccurredOnUtc).ToList();

        var totalIncomes = items.Where(x => x.Type == CashTransactionType.Income).Sum(x => x.Amount);
        var totalExpenses = items.Where(x => x.Type == CashTransactionType.Expense).Sum(x => x.Amount);
        var netBalance = totalIncomes - totalExpenses;

        var response = new FinancialStatementResponse(
            year,
            month,
            totalIncomes,
            totalExpenses,
            netBalance,
            DateTime.UtcNow,
            items);

        return Result<FinancialStatementResponse>.Ok(response);
    }

    private static string CategorizeTransaction(string description, CashTransactionType type)
    {
        var descUpper = description.ToUpperInvariant();
        if (descUpper.Contains("MENSALIDADE") || descUpper.Contains("FATURA") || descUpper.Contains("PIX"))
            return "Mensalidade";
        if (descUpper.Contains("CAMPO") || descUpper.Contains("QUADRA") || descUpper.Contains("ALUGUEL"))
            return "Aluguel de Campo";
        if (descUpper.Contains("COLETE") || descUpper.Contains("BOLA") || descUpper.Contains("EQUIPAMENTO"))
            return "Equipamentos";
        if (descUpper.Contains("CHURRASCO") || descUpper.Contains("FESTA") || descUpper.Contains("EVENTO"))
            return "Confraternização";
        if (descUpper.Contains("ARBITRAGEM") || descUpper.Contains("JUIZ"))
            return "Arbitragem";

        return type == CashTransactionType.Income ? "Outras Receitas" : "Outras Despesas";
    }
}
