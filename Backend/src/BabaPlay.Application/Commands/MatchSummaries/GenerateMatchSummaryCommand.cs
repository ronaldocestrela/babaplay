using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchSummaries;

public sealed record GenerateMatchSummaryCommand(Guid MatchId)
    : ICommand<Result<MatchSummaryResponse>>;
