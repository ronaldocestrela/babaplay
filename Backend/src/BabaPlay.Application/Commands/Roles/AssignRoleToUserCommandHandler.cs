using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Roles;

public sealed class AssignRoleToUserCommandHandler : ICommandHandler<AssignRoleToUserCommand, Result>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly ITenantContext _tenantContext;

    public AssignRoleToUserCommandHandler(
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IUserRepository userRepository,
        IUserTenantRepository userTenantRepository,
        ITenantContext tenantContext)
    {
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _userRepository = userRepository;
        _userTenantRepository = userTenantRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> HandleAsync(AssignRoleToUserCommand cmd, CancellationToken ct = default)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return Result.Fail("TENANT_NOT_RESOLVED", "Tenant must be resolved before role operations.");

        if (string.IsNullOrWhiteSpace(cmd.UserId))
            return Result.Fail("USER_ID_REQUIRED", "UserId is required.");

        if (cmd.RoleId == Guid.Empty)
            return Result.Fail("ROLE_ID_REQUIRED", "RoleId is required.");

        var user = await _userRepository.FindByIdAsync(cmd.UserId.Trim(), ct);
        if (user is null)
            return Result.Fail("USER_NOT_FOUND", $"User '{cmd.UserId}' was not found.");

        var isTenantMember = await _userTenantRepository.IsMemberAsync(cmd.UserId.Trim(), _tenantContext.TenantId, ct);
        if (!isTenantMember)
            return Result.Fail("USER_NOT_IN_TENANT", $"User '{cmd.UserId}' does not belong to current tenant.");

        var role = await _roleRepository.GetByIdAsync(cmd.RoleId, ct);
        if (role is null || !role.IsActive)
            return Result.Fail("ROLE_NOT_FOUND", $"Role '{cmd.RoleId}' was not found.");

        if (await _userRoleRepository.ExistsAsync(cmd.UserId.Trim(), cmd.RoleId, ct))
            return Result.Fail("ROLE_ALREADY_ASSIGNED", $"Role is already assigned to user '{cmd.UserId}'.");

        await _userRoleRepository.AddAsync(UserRole.Create(cmd.UserId, cmd.RoleId), ct);
        await _userRoleRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
