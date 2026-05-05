using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Financial;

public sealed record RegisterMonthlyFeePaymentCommand(
    Guid MonthlyFeeId,
    decimal Amount,
    DateTime PaidAtUtc,
    string? Notes) : ICommand<Result<MonthlyFeePaymentResponse>>;
