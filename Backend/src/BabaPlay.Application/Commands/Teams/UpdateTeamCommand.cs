using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed record UpdateTeamCommand(
    Guid TeamId,
    string Name,
    int MaxPlayers) : ICommand<Result<TeamResponse>>;
