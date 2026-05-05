using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

public sealed record CashTransactionResponse(
    Guid Id,
    Guid TenantId,
    Guid? PlayerId,
    CashTransactionType Type,
    decimal Amount,
    decimal SignedAmount,
    DateTime OccurredOnUtc,
    string Description,
    bool IsActive,
    DateTime CreatedAt);
