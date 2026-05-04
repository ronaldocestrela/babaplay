using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Scores;

public sealed record GetRankingQuery(
    int Page,
    int PageSize,
    DateTime? FromUtc,
    DateTime? ToUtc)
    : IQuery<Result<IReadOnlyList<RankingEntryResponse>>>;