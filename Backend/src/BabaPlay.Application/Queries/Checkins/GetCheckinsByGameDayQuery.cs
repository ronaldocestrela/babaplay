using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Checkins;

public sealed record GetCheckinsByGameDayQuery(Guid GameDayId)
    : IQuery<Result<IReadOnlyList<CheckinResponse>>>;
