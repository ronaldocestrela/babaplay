namespace BabaPlay.Application.DTOs;

public sealed record MonthlySummaryResponse(
    int Year,
    int Month,
    decimal MonthlyFeesAmount,
    decimal MonthlyFeesPaidAmount,
    decimal MonthlyFeesOpenAmount,
    decimal CashIncome,
    decimal CashExpense,
    decimal CashBalance);
