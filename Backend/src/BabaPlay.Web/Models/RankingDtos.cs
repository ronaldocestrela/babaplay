using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public record RankingEntryDto(
    Guid PlayerId,
    string PlayerName,
    string? Nickname,
    string? PhotoUrl,
    int TotalPoints,
    int MatchesPlayed,
    int Victories,
    int Draws,
    int Losses,
    int RankPosition);

public record TopScorerEntryDto(
    Guid PlayerId,
    string PlayerName,
    string? Nickname,
    string? PhotoUrl,
    int Goals,
    int Assists,
    int MatchesPlayed,
    double GoalsPerMatch);

public record AttendanceEntryDto(
    Guid PlayerId,
    string PlayerName,
    string? Nickname,
    string? PhotoUrl,
    int TotalCheckins,
    int TotalGameDays,
    double AttendancePercentage);
