namespace BabaPlay.Infrastructure.Entities;

/// <summary>
/// Represents an association (tenant) in the SaaS model.
/// Each tenant gets its own isolated database (provisioned in Phase 2).
/// </summary>
public sealed class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe unique identifier used for tenant resolution (e.g. subdomain).</summary>
    public string Slug { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
