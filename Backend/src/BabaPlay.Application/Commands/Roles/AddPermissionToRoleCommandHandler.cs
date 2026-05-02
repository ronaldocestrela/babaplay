using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Roles;

public sealed class AddPermissionToRoleCommandHandler
    : ICommandHandler<AddPermissionToRoleCommand, Result<RoleResponse>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public AddPermissionToRoleCommandHandler(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<RoleResponse>> HandleAsync(AddPermissionToRoleCommand cmd, CancellationToken ct = default)
    {
        if (cmd.RoleId == Guid.Empty)
            return Result<RoleResponse>.Fail("ROLE_ID_REQUIRED", "RoleId is required.");

        if (string.IsNullOrWhiteSpace(cmd.PermissionCode))
            return Result<RoleResponse>.Fail("PERMISSION_CODE_REQUIRED", "Permission code is required.");

        var role = await _roleRepository.GetByIdAsync(cmd.RoleId, ct);
        if (role is null || !role.IsActive)
            return Result<RoleResponse>.Fail("ROLE_NOT_FOUND", $"Role '{cmd.RoleId}' was not found.");

        var normalizedCode = cmd.PermissionCode.Trim().ToUpperInvariant();
        var permission = await _permissionRepository.GetByNormalizedCodeAsync(normalizedCode, ct);

        if (permission is null)
        {
            permission = Permission.Create(cmd.PermissionCode, cmd.PermissionDescription, cmd.IsSystemPermission);
            await _permissionRepository.AddAsync(permission, ct);
            await _permissionRepository.SaveChangesAsync(ct);
        }

        role.AddPermission(permission.Id);
        await _roleRepository.UpdateAsync(role, ct);
        await _roleRepository.SaveChangesAsync(ct);

        return Result<RoleResponse>.Ok(new RoleResponse(
            role.Id,
            role.TenantId,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.Permissions.Select(x => x.PermissionId).ToList()));
    }
}
