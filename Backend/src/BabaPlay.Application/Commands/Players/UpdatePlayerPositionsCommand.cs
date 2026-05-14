using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Players;

public sealed record UpdatePlayerPositionsCommand(
    Guid PlayerId,
    IReadOnlyList<Guid> PositionIds) : ICommand<Result<PlayerPositionsResponse>>;
