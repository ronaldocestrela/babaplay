using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Commands.Players;

/// <summary>
/// Handles updates to an existing player's profile within the current tenant.
/// </summary>
public sealed class UpdatePlayerCommandHandler
    : ICommandHandler<UpdatePlayerCommand, Result<PlayerResponse>>
{
    private readonly IPlayerRepository _playerRepository;

    public UpdatePlayerCommandHandler(IPlayerRepository playerRepository)
        => _playerRepository = playerRepository;

    /// <inheritdoc />
    public async Task<Result<PlayerResponse>> HandleAsync(
        UpdatePlayerCommand cmd,
        CancellationToken ct = default)
    {
        var player = await _playerRepository.GetByIdAsync(cmd.PlayerId, ct);
        if (player is null)
            return Result<PlayerResponse>.Fail("PLAYER_NOT_FOUND", $"Player '{cmd.PlayerId}' was not found.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<PlayerResponse>.Fail("INVALID_NAME", "Player name is required.");

        player.Update(cmd.Name, cmd.Nickname, cmd.Phone, cmd.DateOfBirth);
        await _playerRepository.UpdateAsync(player, ct);
        await _playerRepository.SaveChangesAsync(ct);

        return Result<PlayerResponse>.Ok(ToResponse(player));
    }

    private static PlayerResponse ToResponse(Player p) => new(
        p.Id,
        p.UserId,
        p.Name,
        p.Nickname,
        p.Phone,
        p.DateOfBirth,
        p.PositionIds.ToList(),
        p.IsActive,
        p.CreatedAt);
}
