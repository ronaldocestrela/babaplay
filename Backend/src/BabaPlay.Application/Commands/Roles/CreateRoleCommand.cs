using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Roles;

/// <summary>Command to create a new role in the current tenant.</summary>
public sealed record CreateRoleCommand(string Name, string? Description) : ICommand<Result<RoleResponse>>;
