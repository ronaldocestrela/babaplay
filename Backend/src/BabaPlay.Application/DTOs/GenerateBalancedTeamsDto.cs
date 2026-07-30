using System;
using System.Collections.Generic;

namespace BabaPlay.Application.DTOs;

public record DrawnPlayerApplicationDto(
    Guid PlayerId,
    string Name,
    string? Nickname,
    string PositionName,
    int RatingStars,
    string? PhotoUrl,
    Guid TeamId);

public record DrawnTeamApplicationDto(
    Guid TeamId,
    string Name,
    string ColorHex,
    string BadgeClass,
    List<DrawnPlayerApplicationDto> Players,
    double AverageRating);

public record DrawResultApplicationDto(
    Guid GameDayId,
    List<DrawnTeamApplicationDto> Teams,
    int TotalPlayersCount);
