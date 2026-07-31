using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IAuthApiService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetMeAsync(CancellationToken cancellationToken = default);
}
