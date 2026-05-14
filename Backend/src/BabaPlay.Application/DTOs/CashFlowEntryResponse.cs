using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

public sealed record CashFlowEntryResponse(
    Guid Id,
    Guid? PlayerId,
    CashTransactionType Type,
    decimal Amount,
    decimal SignedAmount,
    DateTime OccurredOnUtc,
    string Description);
