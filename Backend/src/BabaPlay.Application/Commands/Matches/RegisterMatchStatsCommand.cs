using System;
using System.Collections.Generic;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Matches;

public sealed record RegisterMatchStatsCommand(
    Guid MatchId,
    int HomeScore,
    int AwayScore,
    IReadOnlyList<PlayerMatchStatsApplicationDto> PlayerStats) : ICommand<Result<RegisterMatchStatsApplicationDto>>;
