using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public record DrawnPlayerDto(
    Guid PlayerId,
    string Name,
    string? Nickname,
    string PositionName,
    int RatingStars, // 1 to 5
    string? PhotoUrl,
    Guid TeamId);

public record DrawnTeamDto(
    Guid TeamId,
    string Name,
    string ColorHex,
    string BadgeClass,
    List<DrawnPlayerDto> Players,
    double AverageRating);

public record GenerateBalancedTeamsDto(
    Guid GameDayId,
    int NumberOfTeams = 2,
    bool BalanceByPosition = true);

public record DrawResultDto(
    Guid GameDayId,
    List<DrawnTeamDto> Teams,
    int TotalPlayersCount);
