using System;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

public sealed record FinancialStatementItemResponse(
    Guid TransactionId,
    DateTime OccurredOnUtc,
    string Description,
    CashTransactionType Type,
    decimal Amount,
    string Category,
    Guid? PlayerId,
    string? PlayerName);
