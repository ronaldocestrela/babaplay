using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public record PlayerMatchStatsDto(
    Guid PlayerId,
    string PlayerName,
    string? PhotoUrl,
    Guid TeamId,
    int Goals = 0,
    int Assists = 0,
    int YellowCards = 0,
    int RedCards = 0,
    int OwnGoals = 0);

public record MatchScoreboardDto(
    Guid MatchId,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    int HomeScore,
    int AwayScore,
    string Status);

public record RegisterMatchStatsDto(
    Guid MatchId,
    int HomeScore,
    int AwayScore,
    List<PlayerMatchStatsDto> PlayerStats);
