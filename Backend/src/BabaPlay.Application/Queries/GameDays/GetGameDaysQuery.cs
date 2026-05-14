using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Queries.GameDays;

public sealed record GetGameDaysQuery(GameDayStatus? Status)
    : IQuery<Result<IReadOnlyList<GameDayResponse>>>;
