using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed record CreateTeamCommand(
    string Name,
    int MaxPlayers) : ICommand<Result<TeamResponse>>;
