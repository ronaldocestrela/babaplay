using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.GameDays;

public sealed record CreateGameDayCommand(
    string Name,
    DateTime ScheduledAt,
    string? Location,
    string? Description,
    int MaxPlayers,
    GameDayStatus Status = GameDayStatus.Confirmed) : ICommand<Result<GameDayResponse>>;
