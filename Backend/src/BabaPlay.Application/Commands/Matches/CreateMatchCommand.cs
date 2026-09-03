using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.Matches;

public sealed record CreateMatchCommand(
    Guid GameDayId,
    Guid? HomeTeamId,
    Guid? AwayTeamId,
    string? Description,
    MatchStatus? InitialStatus = null)
    : ICommand<Result<MatchResponse>>;