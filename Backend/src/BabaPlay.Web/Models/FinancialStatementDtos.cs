using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public sealed record FinancialStatementItemDto
{
    public Guid TransactionId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public string Description { get; init; } = string.Empty;
    public int Type { get; init; } // 0 = Income, 1 = Expense
    public decimal Amount { get; init; }
    public string Category { get; init; } = string.Empty;
    public Guid? PlayerId { get; init; }
    public string? PlayerName { get; init; }
}

public sealed record FinancialStatementDto
{
    public int PeriodYear { get; init; }
    public int PeriodMonth { get; init; }
    public decimal TotalIncomes { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetBalance { get; init; }
    public DateTime PublishedAtUtc { get; init; }
    public List<FinancialStatementItemDto> Items { get; init; } = new();
}
