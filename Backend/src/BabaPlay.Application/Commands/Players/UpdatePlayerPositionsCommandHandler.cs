using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Players;

public sealed class UpdatePlayerPositionsCommandHandler
    : ICommandHandler<UpdatePlayerPositionsCommand, Result<PlayerPositionsResponse>>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPositionRepository _positionRepository;

    public UpdatePlayerPositionsCommandHandler(IPlayerRepository playerRepository, IPositionRepository positionRepository)
    {
        _playerRepository = playerRepository;
        _positionRepository = positionRepository;
    }

    public async Task<Result<PlayerPositionsResponse>> HandleAsync(UpdatePlayerPositionsCommand cmd, CancellationToken ct = default)
    {
        var player = await _playerRepository.GetByIdAsync(cmd.PlayerId, ct);
        if (player is null)
            return Result<PlayerPositionsResponse>.Fail("PLAYER_NOT_FOUND", $"Player '{cmd.PlayerId}' was not found.");

        var inputIds = cmd.PositionIds.ToList();
        if (inputIds.Any(id => id == Guid.Empty))
            return Result<PlayerPositionsResponse>.Fail("INVALID_POSITION_ID", "PositionIds cannot contain empty values.");

        if (inputIds.Distinct().Count() != inputIds.Count)
            return Result<PlayerPositionsResponse>.Fail("DUPLICATE_POSITIONS", "PositionIds cannot contain duplicates.");

        if (inputIds.Count > 3)
            return Result<PlayerPositionsResponse>.Fail("POSITIONS_LIMIT_EXCEEDED", "A player can have at most 3 positions.");

        var positions = await _positionRepository.GetByIdsAsync(inputIds, ct);
        if (positions.Count != inputIds.Count)
            return Result<PlayerPositionsResponse>.Fail("POSITION_NOT_FOUND", "One or more positions were not found.");

        if (positions.Any(p => !p.IsActive))
            return Result<PlayerPositionsResponse>.Fail("POSITION_NOT_FOUND", "One or more positions were not found.");

        player.SetPositions(inputIds);

        await _playerRepository.UpdateAsync(player, ct);
        await _playerRepository.SaveChangesAsync(ct);

        return Result<PlayerPositionsResponse>.Ok(new PlayerPositionsResponse(
            player.Id,
            player.PositionIds.ToList(),
            player.UpdatedAt));
    }
}
