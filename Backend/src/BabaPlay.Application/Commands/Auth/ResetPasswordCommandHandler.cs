using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Auth;

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, Result>
{
    private readonly IPasswordResetService _passwordResetService;

    public ResetPasswordCommandHandler(IPasswordResetService passwordResetService)
    {
        _passwordResetService = passwordResetService;
    }

    public async Task<Result> HandleAsync(ResetPasswordCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result.Fail("EMAIL_REQUIRED", "Email is required.");
        if (string.IsNullOrWhiteSpace(command.Token))
            return Result.Fail("TOKEN_REQUIRED", "Token is required.");
        if (string.IsNullOrWhiteSpace(command.Password))
            return Result.Fail("PASSWORD_REQUIRED", "New password is required.");

        return await _passwordResetService.ResetPasswordAsync(command.Email, command.Token, command.Password, ct);
    }
}
