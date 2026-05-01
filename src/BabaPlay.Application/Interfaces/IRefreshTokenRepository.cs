using BabaPlay.Application.DTOs;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Persistent store for opaque refresh tokens (revocable, rotatable).
/// </summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(string token, string userId, DateTime expiresAt, CancellationToken cancellationToken = default);
    Task<StoredRefreshTokenDto?> FindAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
}
