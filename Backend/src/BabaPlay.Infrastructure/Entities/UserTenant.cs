namespace BabaPlay.Infrastructure.Entities;

/// <summary>Many-to-many link between a user (global identity) and a tenant (association).</summary>
public sealed class UserTenant
{
    public string UserId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public bool IsOwner { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
