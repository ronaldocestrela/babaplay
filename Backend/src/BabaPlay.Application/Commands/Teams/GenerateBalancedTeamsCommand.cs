using System;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed record GenerateBalancedTeamsCommand(
    Guid GameDayId,
    int NumberOfTeams = 2,
    bool BalanceByPosition = true) : ICommand<Result<DrawResultApplicationDto>>;
