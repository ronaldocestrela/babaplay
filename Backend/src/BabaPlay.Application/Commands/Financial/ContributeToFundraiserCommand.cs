using System;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Financial;

public sealed record ContributeToFundraiserCommand(
    Guid FundraiserId,
    decimal Amount,
    Guid? PlayerId = null,
    string? ContributorName = null) : ICommand<Result<FundraiserResponse>>;
