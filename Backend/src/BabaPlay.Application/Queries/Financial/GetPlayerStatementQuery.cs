using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed record GetPlayerStatementQuery(
    Guid PlayerId,
    DateTime FromUtc,
    DateTime ToUtc) : IQuery<Result<PlayerStatementResponse>>;
