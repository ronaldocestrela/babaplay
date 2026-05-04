using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.GameDays;

public sealed record ChangeGameDayStatusCommand(Guid GameDayId, GameDayStatus Status)
    : ICommand<Result<GameDayResponse>>;
