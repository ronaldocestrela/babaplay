namespace BabaPlay.Infrastructure.Entities;

/// <summary>
/// Persistent, revocable refresh token. One user can have multiple active tokens (multi-device).
/// Rotation: every refresh call revokes the presented token and issues a new one.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    /// <summary>Computed — not persisted.</summary>
    public bool IsRevoked => RevokedAt.HasValue;

    public ApplicationUser User { get; set; } = null!;
}
