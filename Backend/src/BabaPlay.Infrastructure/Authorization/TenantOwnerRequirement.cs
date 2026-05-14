using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Infrastructure.Authorization;

/// <summary>
/// Requires authenticated user to be owner/admin of the tenant resolved for the current request.
/// </summary>
public sealed class TenantOwnerRequirement : IAuthorizationRequirement
{
}