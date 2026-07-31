using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class AuthApiService : IAuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/auth/refresh-token",
            new { RefreshToken = refreshToken },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/forgot-password", dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/reset-password", dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<UserProfileDto?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/v1/auth/me", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken: cancellationToken);
    }
}
