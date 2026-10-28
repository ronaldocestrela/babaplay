using System;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Matches;

public sealed record GetMatchStatsQuery(Guid MatchId)
    : IQuery<Result<RegisterMatchStatsApplicationDto>>;
