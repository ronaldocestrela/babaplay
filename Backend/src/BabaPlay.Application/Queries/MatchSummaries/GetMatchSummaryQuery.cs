using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchSummaries;

public sealed record GetMatchSummaryQuery(Guid SummaryId)
    : IQuery<Result<MatchSummaryResponse>>;
