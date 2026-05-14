using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BabaPlay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Infrastructure.Authorization;

public sealed class TenantMemberAuthorizationHandler : AuthorizationHandler<TenantMemberRequirement>
{
    private readonly ITenantContext _tenantContext;
    private readonly IUserTenantRepository _userTenantRepository;

    public TenantMemberAuthorizationHandler(ITenantContext tenantContext, IUserTenantRepository userTenantRepository)
    {
        _tenantContext = tenantContext;
        _userTenantRepository = userTenantRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantMemberRequirement requirement)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return;

        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var isMember = await _userTenantRepository.IsMemberAsync(userId, _tenantContext.TenantId);
        if (isMember)
            context.Succeed(requirement);
    }
}
