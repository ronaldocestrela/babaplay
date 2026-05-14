using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.GameDays;

public sealed class GetGameDayQueryHandler
    : IQueryHandler<GetGameDayQuery, Result<GameDayResponse>>
{
    private readonly IGameDayRepository _gameDayRepository;

    public GetGameDayQueryHandler(IGameDayRepository gameDayRepository)
        => _gameDayRepository = gameDayRepository;

    public async Task<Result<GameDayResponse>> HandleAsync(GetGameDayQuery query, CancellationToken ct = default)
    {
        var gameDay = await _gameDayRepository.GetByIdAsync(query.GameDayId, ct);
        if (gameDay is null)
            return Result<GameDayResponse>.Fail("GAMEDAY_NOT_FOUND", $"Game day '{query.GameDayId}' was not found.");

        return Result<GameDayResponse>.Ok(new GameDayResponse(
            gameDay.Id,
            gameDay.TenantId,
            gameDay.Name,
            gameDay.ScheduledAt,
            gameDay.Location,
            gameDay.Description,
            gameDay.MaxPlayers,
            gameDay.Status,
            gameDay.IsActive,
            gameDay.CreatedAt));
    }
}
