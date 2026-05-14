using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.GameDays;

public sealed record DeleteGameDayCommand(Guid GameDayId) : ICommand<Result>;
