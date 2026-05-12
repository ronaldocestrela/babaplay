namespace BabaPlay.Infrastructure.Entities;

public sealed class AssociationInvite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string InvitedByUserId { get; set; } = string.Empty;
    public DateTime? AcceptedAtUtc { get; set; }
    public string? AcceptedByUserId { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
