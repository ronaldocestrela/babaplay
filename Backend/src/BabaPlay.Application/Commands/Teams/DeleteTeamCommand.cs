using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed record DeleteTeamCommand(Guid TeamId) : ICommand<Result>;
