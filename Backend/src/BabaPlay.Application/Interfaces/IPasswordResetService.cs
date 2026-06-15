using BabaPlay.Application.Common;

namespace BabaPlay.Application.Interfaces;

public interface IPasswordResetService
{
    Task<Result<string>> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default);
}
