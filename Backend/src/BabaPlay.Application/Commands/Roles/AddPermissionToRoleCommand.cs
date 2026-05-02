using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Roles;

/// <summary>Adds a permission to a role in current tenant scope.</summary>
public sealed record AddPermissionToRoleCommand(
    Guid RoleId,
    string PermissionCode,
    string? PermissionDescription,
    bool IsSystemPermission = true) : ICommand<Result<RoleResponse>>;
