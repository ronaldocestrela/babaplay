using System.Threading.Tasks;
using BabaPlay.Web.Services.State;
using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Web.Authorization;

/// <summary>
/// Client-side UI authorization. The API remains the source of truth for write operations.
/// </summary>
public sealed class ClientPermissionAuthorizationHandler
    : AuthorizationHandler<ClientPermissionRequirement>
{
    private readonly TenantState _tenantState;

    public ClientPermissionAuthorizationHandler(TenantState tenantState)
    {
        _tenantState = tenantState;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClientPermissionRequirement requirement)
    {
        if (requirement.PolicyName == ClientAuthorizationPolicies.CommunicationWrite
            && _tenantState.IsOwner)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
