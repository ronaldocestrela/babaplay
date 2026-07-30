using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public record CheckinDto(
    Guid Id,
    Guid TenantId,
    Guid PlayerId,
    string? PlayerName,
    string? PhotoUrl,
    string? PositionName,
    Guid GameDayId,
    DateTime CheckedInAtUtc,
    double Latitude,
    double Longitude,
    double DistanceFromAssociationMeters,
    bool IsActive,
    string Status);

public record CreateCheckinDto(
    Guid PlayerId,
    Guid GameDayId,
    DateTime CheckedInAtUtc,
    double Latitude,
    double Longitude);

public record RsvpSubmissionDto(
    Guid GameDayId,
    Guid PlayerId,
    bool IsAttending);

public record GameDayRsvpSummaryDto(
    Guid GameDayId,
    string GameDayName,
    DateTime ScheduledAt,
    string? Location,
    int MaxPlayers,
    int ConfirmedCount,
    int WaitlistCount,
    string? UserRsvpStatus, // "Confirmed", "Declined", "Waitlist", null
    int? UserWaitlistPosition);

public record PlayerRsvpDetailDto(
    Guid PlayerId,
    string PlayerName,
    string? Nickname,
    string? PhotoUrl,
    string? PrimaryPositionName,
    int? JerseyNumber,
    string Status, // "Confirmed", "Declined", "Waitlist"
    DateTime ConfirmedAt,
    int? WaitlistOrder);
