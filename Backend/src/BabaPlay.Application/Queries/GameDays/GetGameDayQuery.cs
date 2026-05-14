using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.GameDays;

public sealed record GetGameDayQuery(Guid GameDayId)
    : IQuery<Result<GameDayResponse>>;
