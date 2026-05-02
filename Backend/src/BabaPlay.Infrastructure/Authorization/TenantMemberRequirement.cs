using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Infrastructure.Authorization;

/// <summary>
/// Requires authenticated user to belong to the tenant resolved for the current request.
/// </summary>
public sealed class TenantMemberRequirement : IAuthorizationRequirement
{
}
