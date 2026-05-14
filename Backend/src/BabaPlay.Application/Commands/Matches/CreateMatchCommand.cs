using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Matches;

public sealed record CreateMatchCommand(
    Guid GameDayId,
    Guid? HomeTeamId,
    Guid? AwayTeamId,
    string? Description)
    : ICommand<Result<MatchResponse>>;