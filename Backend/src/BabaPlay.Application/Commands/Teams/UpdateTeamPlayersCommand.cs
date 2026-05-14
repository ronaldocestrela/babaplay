using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed record UpdateTeamPlayersCommand(
    Guid TeamId,
    IReadOnlyList<Guid> PlayerIds) : ICommand<Result<TeamPlayersResponse>>;