using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Financial;

public sealed record CreatePlayerMonthlyFeeCommand(
    Guid PlayerId,
    int Year,
    int Month,
    decimal Amount,
    DateTime DueDateUtc,
    string? Description) : ICommand<Result<PlayerMonthlyFeeResponse>>;
