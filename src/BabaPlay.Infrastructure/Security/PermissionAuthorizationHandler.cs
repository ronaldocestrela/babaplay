using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Infrastructure.Security;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.Permission) ||
            context.User.HasClaim("permission", "*"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
