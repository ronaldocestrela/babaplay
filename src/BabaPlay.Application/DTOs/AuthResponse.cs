namespace BabaPlay.Application.DTOs;

/// <summary>Authentication response returned after a successful login or token refresh.</summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType = "Bearer");
