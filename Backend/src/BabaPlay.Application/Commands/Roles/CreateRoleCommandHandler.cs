using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Roles;

public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Result<RoleResponse>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ITenantContext _tenantContext;

    public CreateRoleCommandHandler(IRoleRepository roleRepository, ITenantContext tenantContext)
    {
        _roleRepository = roleRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<RoleResponse>> HandleAsync(CreateRoleCommand cmd, CancellationToken ct = default)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return Result<RoleResponse>.Fail("TENANT_NOT_RESOLVED", "Tenant must be resolved before role operations.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<RoleResponse>.Fail("ROLE_NAME_REQUIRED", "Role name is required.");

        var normalizedName = cmd.Name.Trim().ToUpperInvariant();
        if (await _roleRepository.ExistsByNormalizedNameAsync(normalizedName, ct))
            return Result<RoleResponse>.Fail("ROLE_ALREADY_EXISTS", $"Role '{cmd.Name}' already exists in this tenant.");

        var role = Role.Create(_tenantContext.TenantId, cmd.Name, cmd.Description);
        await _roleRepository.AddAsync(role, ct);
        await _roleRepository.SaveChangesAsync(ct);

        return Result<RoleResponse>.Ok(ToResponse(role));
    }

    private static RoleResponse ToResponse(Role role) => new(
        role.Id,
        role.TenantId,
        role.Name,
        role.Description,
        role.IsActive,
        role.CreatedAt,
        role.Permissions.Select(x => x.PermissionId).ToList());
}
