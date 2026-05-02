using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Roles;

/// <summary>Assigns an existing tenant role to a user belonging to the same tenant.</summary>
public sealed record AssignRoleToUserCommand(string UserId, Guid RoleId) : ICommand<Result>;
