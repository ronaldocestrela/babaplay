using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Players;

/// <summary>
/// Handles player creation within the current tenant's database.
/// Validates that the referenced ApplicationUser exists (Master DB)
/// and that the user is not already registered as a player in this tenant.
/// </summary>
public sealed class CreatePlayerCommandHandler
    : ICommandHandler<CreatePlayerCommand, Result<PlayerResponse>>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IUserRepository _userRepository;

    public CreatePlayerCommandHandler(
        IPlayerRepository playerRepository,
        IUserRepository userRepository)
    {
        _playerRepository = playerRepository;
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<Result<PlayerResponse>> HandleAsync(
        CreatePlayerCommand cmd,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<PlayerResponse>.Fail("INVALID_NAME", "Player name is required.");

        var user = await _userRepository.FindByIdAsync(cmd.UserId.ToString(), ct);
        if (user is null)
            return Result<PlayerResponse>.Fail("USER_NOT_FOUND", $"User '{cmd.UserId}' was not found.");

        if (await _playerRepository.ExistsByUserIdAsync(cmd.UserId, ct))
            return Result<PlayerResponse>.Fail(
                "PLAYER_ALREADY_EXISTS",
                $"A player for user '{cmd.UserId}' already exists in this tenant.");

        var player = Player.Create(cmd.UserId, cmd.Name, cmd.Nickname, cmd.Phone, cmd.DateOfBirth);
        await _playerRepository.AddAsync(player, ct);
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
        p.IsActive,
        p.CreatedAt);
}
