using System.Security.Claims;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Security;
using Microsoft.AspNetCore.Identity;

namespace BabaPlay.Modules.Identity.Services;

public sealed class AuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IPermissionResolver _permissions;
    private readonly IAccessTokenIssuer _tokens;

    public AuthService(UserManager<ApplicationUser> users, IPermissionResolver permissions, IAccessTokenIssuer tokens)
    {
        _users = users;
        _permissions = permissions;
        _tokens = tokens;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        string email,
        string password,
        UserType userType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return Result.Invalid<AuthResponse>("Email is required.");
        if (string.IsNullOrWhiteSpace(password)) return Result.Invalid<AuthResponse>("Password is required.");

        var user = new ApplicationUser
        {
            Email = email.Trim(),
            UserName = email.Trim(),
            UserType = userType
        };

        var res = await _users.CreateAsync(user, password);
        if (!res.Succeeded)
            return Result.Invalid<AuthResponse>(res.Errors.Select(e => e.Description));

        var role = userType switch
        {
            UserType.PlatformAdmin => "Admin",
            UserType.AssociationStaff => "Manager",
            _ => "Associate"
        };

        await _users.AddToRoleAsync(user, role);
        return await BuildTokenAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return Result.Invalid<AuthResponse>("Email is required.");
        var user = await _users.FindByEmailAsync(email.Trim());
        if (user is null) return Result.Unauthorized<AuthResponse>("Invalid credentials.");
        if (!await _users.CheckPasswordAsync(user, password))
            return Result.Unauthorized<AuthResponse>("Invalid credentials.");
        return await BuildTokenAsync(user, cancellationToken);
    }

    private async Task<Result<AuthResponse>> BuildTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await _users.GetRolesAsync(user);
        var perms = await _permissions.GetPermissionNamesForUserAsync(user.Id, cancellationToken);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new("sub", user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id)
        };
        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(perms.Select(p => new Claim("permission", p)));

        var token = _tokens.Issue(claims);
        return Result.Success(new AuthResponse(token, user.Id, roles.ToList(), perms));
    }
}

public sealed record AuthResponse(string AccessToken, string UserId, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);
