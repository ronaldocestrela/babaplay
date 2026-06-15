using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace BabaPlay.Infrastructure.Services;

public sealed class PasswordResetService : IPasswordResetService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PasswordResetService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<string>> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return Result<string>.Fail("USER_NOT_FOUND", "User not found.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Result<string>.Ok(token);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return Result.Fail("USER_NOT_FOUND", "User not found.");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail("RESET_PASSWORD_FAILED", errors);
        }

        return Result.Ok();
    }
}
