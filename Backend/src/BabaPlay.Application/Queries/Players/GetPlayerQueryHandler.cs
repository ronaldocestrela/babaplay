using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Queries.Players;

/// <summary>Handles retrieval of a single player by id.</summary>
public sealed class GetPlayerQueryHandler
    : IQueryHandler<GetPlayerQuery, Result<PlayerResponse>>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerQueryHandler(IPlayerRepository playerRepository)
        => _playerRepository = playerRepository;

    /// <inheritdoc />
    public async Task<Result<PlayerResponse>> HandleAsync(
        GetPlayerQuery query,
        CancellationToken ct = default)
    {
        var player = await _playerRepository.GetByIdAsync(query.PlayerId, ct);

        if (player is null)
            return Result<PlayerResponse>.Fail("PLAYER_NOT_FOUND", $"Player '{query.PlayerId}' was not found.");

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
