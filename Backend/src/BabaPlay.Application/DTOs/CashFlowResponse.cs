namespace BabaPlay.Application.DTOs;

public sealed record CashFlowResponse(
    DateTime FromUtc,
    DateTime ToUtc,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    IReadOnlyList<CashFlowEntryResponse> Entries);
