using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.GameDays;

public sealed class DeleteGameDayCommandHandler
    : ICommandHandler<DeleteGameDayCommand, Result>
{
    private readonly IGameDayRepository _gameDayRepository;

    public DeleteGameDayCommandHandler(IGameDayRepository gameDayRepository)
        => _gameDayRepository = gameDayRepository;

    public async Task<Result> HandleAsync(DeleteGameDayCommand cmd, CancellationToken ct = default)
    {
        var gameDay = await _gameDayRepository.GetByIdAsync(cmd.GameDayId, ct);
        if (gameDay is null)
            return Result.Fail("GAMEDAY_NOT_FOUND", $"Game day '{cmd.GameDayId}' was not found.");

        gameDay.Deactivate();
        await _gameDayRepository.UpdateAsync(gameDay, ct);
        await _gameDayRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
