using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace BabaPlay.Infrastructure.Services;

public sealed class UserInvitationAccountService : IUserInvitationAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserInvitationAccountService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var existing = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null)
            return Result<string>.Fail("ASSOCIATION_INVITE_EMAIL_ALREADY_REGISTERED", "This e-mail is already registered.");

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            IsActive = true,
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var details = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<string>.Fail("ASSOCIATION_INVITE_USER_CREATE_FAILED", details);
        }

        return Result<string>.Ok(user.Id);
    }
}
