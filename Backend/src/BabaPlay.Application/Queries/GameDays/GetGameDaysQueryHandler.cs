using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.GameDays;

public sealed class GetGameDaysQueryHandler
    : IQueryHandler<GetGameDaysQuery, Result<IReadOnlyList<GameDayResponse>>>
{
    private readonly IGameDayRepository _gameDayRepository;

    public GetGameDaysQueryHandler(IGameDayRepository gameDayRepository)
        => _gameDayRepository = gameDayRepository;

    public async Task<Result<IReadOnlyList<GameDayResponse>>> HandleAsync(GetGameDaysQuery query, CancellationToken ct = default)
    {
        var gameDays = await _gameDayRepository.GetAllActiveAsync(query.Status, ct);

        return Result<IReadOnlyList<GameDayResponse>>.Ok(gameDays
            .Select(gameDay => new GameDayResponse(
                gameDay.Id,
                gameDay.TenantId,
                gameDay.Name,
                gameDay.ScheduledAt,
                gameDay.Location,
                gameDay.Description,
                gameDay.MaxPlayers,
                gameDay.Status,
                gameDay.IsActive,
                gameDay.CreatedAt))
            .ToList());
    }
}
