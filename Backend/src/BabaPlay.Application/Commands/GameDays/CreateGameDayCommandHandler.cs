using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.GameDays;

public sealed class CreateGameDayCommandHandler
    : ICommandHandler<CreateGameDayCommand, Result<GameDayResponse>>
{
    private readonly IGameDayRepository _gameDayRepository;
    private readonly ITenantContext _tenantContext;

    public CreateGameDayCommandHandler(IGameDayRepository gameDayRepository, ITenantContext tenantContext)
    {
        _gameDayRepository = gameDayRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<GameDayResponse>> HandleAsync(CreateGameDayCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<GameDayResponse>.Fail("INVALID_NAME", "Game day name is required.");

        if (cmd.ScheduledAt <= DateTime.UtcNow)
            return Result<GameDayResponse>.Fail("INVALID_SCHEDULED_AT", "ScheduledAt must be in the future.");

        if (cmd.MaxPlayers <= 0)
            return Result<GameDayResponse>.Fail("INVALID_MAX_PLAYERS", "MaxPlayers must be greater than zero.");

        var normalizedName = cmd.Name.Trim().ToUpperInvariant();
        var exists = await _gameDayRepository.ExistsByNormalizedNameAndScheduledAtAsync(normalizedName, cmd.ScheduledAt, ct);
        if (exists)
            return Result<GameDayResponse>.Fail("GAMEDAY_ALREADY_EXISTS", "A game day with the same name and schedule already exists.");

        var gameDay = GameDay.Create(
            _tenantContext.TenantId,
            cmd.Name,
            cmd.ScheduledAt,
            cmd.Location,
            cmd.Description,
            cmd.MaxPlayers);

        await _gameDayRepository.AddAsync(gameDay, ct);
        await _gameDayRepository.SaveChangesAsync(ct);

        return Result<GameDayResponse>.Ok(ToResponse(gameDay));
    }

    private static GameDayResponse ToResponse(GameDay gameDay) => new(
        gameDay.Id,
        gameDay.TenantId,
        gameDay.Name,
        gameDay.ScheduledAt,
        gameDay.Location,
        gameDay.Description,
        gameDay.MaxPlayers,
        gameDay.Status,
        gameDay.IsActive,
        gameDay.CreatedAt);
}
