using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Queries.Players;

/// <summary>Handles retrieval of all active players for the current tenant.</summary>
public sealed class GetPlayersQueryHandler
    : IQueryHandler<GetPlayersQuery, Result<IReadOnlyList<PlayerResponse>>>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayersQueryHandler(IPlayerRepository playerRepository)
        => _playerRepository = playerRepository;

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PlayerResponse>>> HandleAsync(
        GetPlayersQuery query,
        CancellationToken ct = default)
    {
        var players = await _playerRepository.GetAllActiveAsync(ct);
        var response = players.Select(ToResponse).ToList();
        return Result<IReadOnlyList<PlayerResponse>>.Ok(response);
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
