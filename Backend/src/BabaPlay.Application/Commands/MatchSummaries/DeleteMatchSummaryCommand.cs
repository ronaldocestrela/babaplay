using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchSummaries;

public sealed record DeleteMatchSummaryCommand(Guid SummaryId)
    : ICommand<Result>;
