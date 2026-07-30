using System;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Financial;

public sealed record CreateFundraiserCommand(
    string Title,
    string Description,
    decimal TargetAmount,
    DateTime? DeadlineUtc = null) : ICommand<Result<FundraiserResponse>>;
