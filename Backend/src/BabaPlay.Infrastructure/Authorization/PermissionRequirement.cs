using Microsoft.AspNetCore.Authorization;

namespace BabaPlay.Infrastructure.Authorization;

/// <summary>
/// Requires an authenticated user to have a specific permission code in current tenant scope.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode.Trim().ToUpperInvariant();
    }

    public string PermissionCode { get; }
}
