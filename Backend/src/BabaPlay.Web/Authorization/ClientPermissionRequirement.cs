using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Web.Authorization;

public sealed class ClientPermissionRequirement : IAuthorizationRequirement
{
    public ClientPermissionRequirement(string policyName)
    {
        PolicyName = policyName;
    }

    public string PolicyName { get; }
}
