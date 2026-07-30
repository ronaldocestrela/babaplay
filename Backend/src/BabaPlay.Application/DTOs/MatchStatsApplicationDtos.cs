using System;
using System.Collections.Generic;

namespace BabaPlay.Application.DTOs;

public record PlayerMatchStatsApplicationDto(
    Guid PlayerId,
    string PlayerName,
    string? PhotoUrl,
    Guid TeamId,
    int Goals,
    int Assists,
    int YellowCards,
    int RedCards,
    int OwnGoals);

public record RegisterMatchStatsApplicationDto(
    Guid MatchId,
    int HomeScore,
    int AwayScore,
    List<PlayerMatchStatsApplicationDto> PlayerStats);
