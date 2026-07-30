using System;
using System.Collections.Generic;

namespace BabaPlay.Application.DTOs;

public sealed record FinancialStatementResponse(
    int PeriodYear,
    int PeriodMonth,
    decimal TotalIncomes,
    decimal TotalExpenses,
    decimal NetBalance,
    DateTime PublishedAtUtc,
    IReadOnlyList<FinancialStatementItemResponse> Items);
