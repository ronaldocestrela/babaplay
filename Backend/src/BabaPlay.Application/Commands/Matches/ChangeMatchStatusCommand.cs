using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.Matches;

public sealed record ChangeMatchStatusCommand(Guid MatchId, MatchStatus Status)
    : ICommand<Result<MatchResponse>>;