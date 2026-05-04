using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Queries.Matches;

public sealed record GetMatchesQuery(MatchStatus? Status)
    : IQuery<Result<IReadOnlyList<MatchResponse>>>;