using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BabaPlay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ITenantContext _tenantContext;
    private readonly IUserRoleRepository _userRoleRepository;

    public PermissionAuthorizationHandler(ITenantContext tenantContext, IUserRoleRepository userRoleRepository)
    {
        _tenantContext = tenantContext;
        _userRoleRepository = userRoleRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return;

        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var hasPermission = await _userRoleRepository.HasPermissionAsync(userId, requirement.PermissionCode);
        if (hasPermission)
            context.Succeed(requirement);
    }
}
