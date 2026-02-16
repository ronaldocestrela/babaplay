using Application.Exceptions;
using Application.Features.Identity.Users;
using System.Security.Claims;

namespace Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private ClaimsPrincipal? _principal;

    public string Name => _principal?.Identity?.Name ?? string.Empty;

    public IEnumerable<Claim> GetUserClaims()
    {
        return _principal?.Claims ?? Enumerable.Empty<Claim>();
    }

    public string GetUserEmail()
    {
        return _principal?.GetEmail() ?? string.Empty;
    }

    public string GetUserId()
    {
        return _principal?.GetUserId() ?? string.Empty;
    }

    public string GetUserTenant()
    {
        return _principal?.GetTenant() ?? string.Empty;
    }

    public bool IsAuthenticated()
    {
        return _principal?.Identity?.IsAuthenticated ?? false;
    }

    public bool IsInRole(string roleName)
    {
        return _principal?.IsInRole(roleName) ?? false;
    }

    public void SetCurrentUser(ClaimsPrincipal principal)
    {
        // overwrite previous principal if any — safer for middleware re-entry
        _principal = principal;
    }
}
