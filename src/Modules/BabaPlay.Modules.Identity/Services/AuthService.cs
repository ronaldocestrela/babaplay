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
    private readonly IAssociateStatusChecker _associateStatus;
    private readonly IAssociateSignupSynchronizer _associateSignup;
    private readonly IAssociateInvitationService _associateInvitations;

    public AuthService(
        UserManager<ApplicationUser> users,
        IPermissionResolver permissions,
        IAccessTokenIssuer tokens,
        IAssociateStatusChecker associateStatus,
        IAssociateSignupSynchronizer associateSignup,
        IAssociateInvitationService associateInvitations)
    {
        _users = users;
        _permissions = permissions;
        _tokens = tokens;
        _associateStatus = associateStatus;
        _associateSignup = associateSignup;
        _associateInvitations = associateInvitations;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        string name,
        string email,
        string password,
        UserType userType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<AuthResponse>("Name is required.");
        if (string.IsNullOrWhiteSpace(email)) return Result.Invalid<AuthResponse>("Email is required.");
        if (string.IsNullOrWhiteSpace(password)) return Result.Invalid<AuthResponse>("Password is required.");

        var normalizedName = name.Trim();
        var normalizedEmail = email.Trim();

        var user = new ApplicationUser
        {
            Email = normalizedEmail,
            UserName = normalizedEmail,
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

        var roleResult = await _users.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _users.DeleteAsync(user);
            return Result.Invalid<AuthResponse>(roleResult.Errors.Select(e => e.Description));
        }

        if (userType is UserType.AssociationStaff or UserType.Associate)
        {
            var associateResult = await _associateSignup.CreateAsync(normalizedName, normalizedEmail, user.Id, cancellationToken);
            if (!associateResult.IsSuccess)
            {
                await _users.DeleteAsync(user);
                return Result.Invalid<AuthResponse>(associateResult.Errors);
            }

            user.AssociateId = associateResult.Value;
            var updateUserResult = await _users.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                await _associateSignup.DeleteAsync(associateResult.Value, cancellationToken);
                await _users.DeleteAsync(user);
                return Result.Invalid<AuthResponse>(updateUserResult.Errors.Select(e => e.Description));
            }
        }

        return await BuildTokenAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return Result.Invalid<AuthResponse>("Email is required.");
        var user = await _users.FindByEmailAsync(email.Trim());
        if (user is null) return Result.Unauthorized<AuthResponse>("Invalid credentials.");
        if (!await _users.CheckPasswordAsync(user, password))
            return Result.Unauthorized<AuthResponse>("Invalid credentials.");
        if (!await _associateStatus.IsActiveByUserIdAsync(user.Id, cancellationToken))
            return Result.Forbidden<AuthResponse>("Associate account is inactive.");
        return await BuildTokenAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RegisterWithInvitationAsync(
        string invitationToken,
        string name,
        string? email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(invitationToken))
            return Result.Invalid<AuthResponse>("Invitation token is required.");

        var invitation = await _associateInvitations.ValidateAsync(invitationToken, cancellationToken);
        if (invitation.IsFailure)
            return FailFromResult<AuthResponse>(invitation);

        var registrationEmail = invitation.Value.IsSingleUse
            ? invitation.Value.Email
            : email;

        if (string.IsNullOrWhiteSpace(registrationEmail))
            return Result.Invalid<AuthResponse>("Email is required.");

        var register = await RegisterAsync(
            name,
            registrationEmail,
            password,
            UserType.Associate,
            cancellationToken);

        if (register.IsFailure)
            return register;

        var accepted = await _associateInvitations.MarkAcceptedAsync(
            invitationToken,
            register.Value.UserId,
            cancellationToken);

        if (accepted.IsSuccess)
            return register;

        await RollbackRegisteredAssociateAsync(register.Value.UserId, cancellationToken);
        return FailFromResult<AuthResponse>(accepted);
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

    private async Task RollbackRegisteredAssociateAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return;

        if (!string.IsNullOrWhiteSpace(user.AssociateId))
            await _associateSignup.DeleteAsync(user.AssociateId, cancellationToken);

        await _users.DeleteAsync(user);
    }

    private static Result<T> FailFromResult<T>(Result result)
    {
        if (result.Errors.Count > 0)
            return Result.Fail<T>(result.Errors, result.Status);

        if (!string.IsNullOrWhiteSpace(result.Error))
            return Result.Fail<T>(result.Error, result.Status);

        return Result.Fail<T>("Operation failed.", result.Status);
    }
}

public sealed record AuthResponse(string AccessToken, string UserId, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);
