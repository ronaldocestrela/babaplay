using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchEvents;

public sealed record GetMatchEventTypeQuery(Guid MatchEventTypeId) : IQuery<Result<MatchEventTypeResponse>>;
