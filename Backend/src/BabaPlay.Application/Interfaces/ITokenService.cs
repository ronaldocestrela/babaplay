namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Token generation and metadata. Implemented in Infrastructure; Application depends only on this contract.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a signed JWT access token.</summary>
    string GenerateAccessToken(string userId, string email, IReadOnlyCollection<string> roles);

    /// <summary>Generates a cryptographically-random opaque refresh token.</summary>
    string GenerateRefreshToken();

    int AccessTokenExpiresInSeconds { get; }
    int RefreshTokenExpiresInDays { get; }
}
