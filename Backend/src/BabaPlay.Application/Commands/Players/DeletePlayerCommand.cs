using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Players;

/// <summary>Command to soft-delete (deactivate) a player within the current tenant.</summary>
public sealed record DeletePlayerCommand(Guid PlayerId) : ICommand<Result>;
