using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.Financial;

public sealed record CreateCashTransactionCommand(
    CashTransactionType Type,
    decimal Amount,
    DateTime OccurredOnUtc,
    string Description,
    Guid? PlayerId) : ICommand<Result<CashTransactionResponse>>;
