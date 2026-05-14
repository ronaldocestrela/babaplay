namespace BabaPlay.Application.DTOs;

public sealed record MonthlyFeePaymentResponse(
    Guid Id,
    Guid TenantId,
    Guid MonthlyFeeId,
    decimal Amount,
    DateTime PaidAtUtc,
    string? Notes,
    bool IsReversed,
    DateTime? ReversedAtUtc,
    bool IsActive,
    DateTime CreatedAt);
