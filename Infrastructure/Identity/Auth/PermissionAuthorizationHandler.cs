using BabaPlayShared.Library.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity.Auth;

public class PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger) : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger = logger;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        try
        {
            _logger.LogInformation("Authorization check for permission={Permission} - ClaimsCount={Count}", requirement.Permission, context.User?.Claims?.Count() ?? 0);

            var permissions = context.User.Claims
                .Where(claim => claim.Type == ClaimConstants.Permission
                    && claim.Value == requirement.Permission);

            if (permissions.Any())
            {
                context.Succeed(requirement);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during permission authorization");
            throw;
        }

        await Task.CompletedTask;
    }
}
