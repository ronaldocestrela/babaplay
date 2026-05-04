using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Matches;

public sealed record GetMatchQuery(Guid MatchId)
    : IQuery<Result<MatchResponse>>;