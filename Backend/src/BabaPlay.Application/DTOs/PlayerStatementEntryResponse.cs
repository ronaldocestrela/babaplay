using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

public sealed record PlayerStatementEntryResponse(
    Guid MonthlyFeeId,
    int Year,
    int Month,
    decimal Amount,
    decimal PaidAmount,
    DateTime DueDateUtc,
    DateTime? PaidAtUtc,
    MonthlyFeeStatus Status,
    decimal OpenAmount);
