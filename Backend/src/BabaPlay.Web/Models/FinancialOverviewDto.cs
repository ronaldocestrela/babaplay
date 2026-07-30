namespace BabaPlay.Web.Models;

public sealed record FinancialOverviewDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal NetBalance { get; init; }
    public decimal PendingMonthlyFeesAmount { get; init; }
    public int DelinquentPlayersCount { get; init; }
    public decimal CollectionRatePercentage { get; init; }
    public List<RecentTransactionDto> RecentTransactions { get; init; } = new();
}

public sealed record RecentTransactionDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? PlayerId { get; init; }
    public int Type { get; init; } // 1: Income, 2: Expense
    public decimal Amount { get; init; }
    public decimal SignedAmount { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
