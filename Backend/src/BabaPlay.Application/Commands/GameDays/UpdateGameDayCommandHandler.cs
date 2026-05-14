using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.GameDays;

public sealed class UpdateGameDayCommandHandler
    : ICommandHandler<UpdateGameDayCommand, Result<GameDayResponse>>
{
    private readonly IGameDayRepository _gameDayRepository;

    public UpdateGameDayCommandHandler(IGameDayRepository gameDayRepository)
        => _gameDayRepository = gameDayRepository;

    public async Task<Result<GameDayResponse>> HandleAsync(UpdateGameDayCommand cmd, CancellationToken ct = default)
    {
        var gameDay = await _gameDayRepository.GetByIdAsync(cmd.GameDayId, ct);
        if (gameDay is null)
            return Result<GameDayResponse>.Fail("GAMEDAY_NOT_FOUND", $"Game day '{cmd.GameDayId}' was not found.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<GameDayResponse>.Fail("INVALID_NAME", "Game day name is required.");

        if (cmd.ScheduledAt <= DateTime.UtcNow)
            return Result<GameDayResponse>.Fail("INVALID_SCHEDULED_AT", "ScheduledAt must be in the future.");

        if (cmd.MaxPlayers <= 0)
            return Result<GameDayResponse>.Fail("INVALID_MAX_PLAYERS", "MaxPlayers must be greater than zero.");

        var normalizedName = cmd.Name.Trim().ToUpperInvariant();
        if (!(string.Equals(gameDay.NormalizedName, normalizedName, StringComparison.Ordinal)
            && gameDay.ScheduledAt == cmd.ScheduledAt))
        {
            var exists = await _gameDayRepository.ExistsByNormalizedNameAndScheduledAtAsync(normalizedName, cmd.ScheduledAt, ct);
            if (exists)
                return Result<GameDayResponse>.Fail("GAMEDAY_ALREADY_EXISTS", "A game day with the same name and schedule already exists.");
        }

        gameDay.Update(cmd.Name, cmd.ScheduledAt, cmd.Location, cmd.Description, cmd.MaxPlayers);
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
