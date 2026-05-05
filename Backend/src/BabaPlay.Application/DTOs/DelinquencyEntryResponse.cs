namespace BabaPlay.Application.DTOs;

public sealed record DelinquencyEntryResponse(
    Guid MonthlyFeeId,
    Guid PlayerId,
    int Year,
    int Month,
    decimal Amount,
    decimal PaidAmount,
    decimal OpenAmount,
    DateTime DueDateUtc,
    int DaysOverdue);
