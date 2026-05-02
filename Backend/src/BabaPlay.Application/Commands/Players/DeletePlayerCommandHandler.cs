using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Players;

/// <summary>
/// Handles soft-deletion of a player by setting <c>IsActive = false</c>.
/// Idempotent: succeeds even if the player is already deactivated.
/// </summary>
public sealed class DeletePlayerCommandHandler
    : ICommandHandler<DeletePlayerCommand, Result>
{
    private readonly IPlayerRepository _playerRepository;

    public DeletePlayerCommandHandler(IPlayerRepository playerRepository)
        => _playerRepository = playerRepository;

    /// <inheritdoc />
    public async Task<Result> HandleAsync(
        DeletePlayerCommand cmd,
        CancellationToken ct = default)
    {
        var player = await _playerRepository.GetByIdAsync(cmd.PlayerId, ct);
        if (player is null)
            return Result.Fail("PLAYER_NOT_FOUND", $"Player '{cmd.PlayerId}' was not found.");

        player.Deactivate();
        await _playerRepository.UpdateAsync(player, ct);
        await _playerRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
