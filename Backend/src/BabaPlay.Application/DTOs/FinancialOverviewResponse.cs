namespace BabaPlay.Application.DTOs;

public sealed record FinancialOverviewResponse(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance,
    decimal PendingMonthlyFeesAmount,
    int DelinquentPlayersCount,
    decimal CollectionRatePercentage,
    IReadOnlyList<CashTransactionResponse> RecentTransactions);
