using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchEvents;

public sealed record GetMatchEventQuery(Guid MatchEventId) : IQuery<Result<MatchEventResponse>>;
