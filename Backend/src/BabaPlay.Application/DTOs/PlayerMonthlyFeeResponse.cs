using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

public sealed record PlayerMonthlyFeeResponse(
    Guid Id,
    Guid TenantId,
    Guid PlayerId,
    int Year,
    int Month,
    decimal Amount,
    decimal PaidAmount,
    DateTime DueDateUtc,
    DateTime? PaidAtUtc,
    MonthlyFeeStatus Status,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);
