using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Checkins;

public sealed record CreateCheckinCommand(
    Guid PlayerId,
    Guid GameDayId,
    DateTime CheckedInAtUtc,
    double Latitude,
    double Longitude) : ICommand<Result<CheckinResponse>>;
