using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BabaPlay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Infrastructure.Authorization;

public sealed class TenantOwnerAuthorizationHandler : AuthorizationHandler<TenantOwnerRequirement>
{
    private readonly ITenantContext _tenantContext;
    private readonly IUserTenantRepository _userTenantRepository;

    public TenantOwnerAuthorizationHandler(ITenantContext tenantContext, IUserTenantRepository userTenantRepository)
    {
        _tenantContext = tenantContext;
        _userTenantRepository = userTenantRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantOwnerRequirement requirement)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return;

        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var isOwner = await _userTenantRepository.IsOwnerAsync(userId, _tenantContext.TenantId);
        if (isOwner)
            context.Succeed(requirement);
    }
}