namespace BabaPlay.Application.DTOs;

/// <summary>Projection of a persisted refresh token entry returned by the repository.</summary>
public record StoredRefreshTokenDto(string Token, string UserId, DateTime ExpiresAt, bool IsRevoked);
