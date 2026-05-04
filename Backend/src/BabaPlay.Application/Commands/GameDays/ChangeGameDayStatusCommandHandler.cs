using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Commands.GameDays;

public sealed class ChangeGameDayStatusCommandHandler
    : ICommandHandler<ChangeGameDayStatusCommand, Result<GameDayResponse>>
{
    private readonly IGameDayRepository _gameDayRepository;

    public ChangeGameDayStatusCommandHandler(IGameDayRepository gameDayRepository)
        => _gameDayRepository = gameDayRepository;

    public async Task<Result<GameDayResponse>> HandleAsync(ChangeGameDayStatusCommand cmd, CancellationToken ct = default)
    {
        var gameDay = await _gameDayRepository.GetByIdAsync(cmd.GameDayId, ct);
        if (gameDay is null)
            return Result<GameDayResponse>.Fail("GAMEDAY_NOT_FOUND", $"Game day '{cmd.GameDayId}' was not found.");

        try
        {
            gameDay.ChangeStatus(cmd.Status);
        }
        catch (ValidationException)
        {
            return Result<GameDayResponse>.Fail("INVALID_STATUS_TRANSITION", "Invalid game day status transition.");
        }

        await _gameDayRepository.UpdateAsync(gameDay, ct);
        await _gameDayRepository.SaveChangesAsync(ct);

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
