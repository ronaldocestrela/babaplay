namespace BabaPlay.Infrastructure.Entities;

/// <summary>
/// Represents an association (tenant) in the SaaS model.
/// Tenant-scoped data isolation is logical via TenantId in single-db mode.
/// </summary>
public sealed class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe unique identifier used for tenant resolution (e.g. subdomain).</summary>
    public string Slug { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public int PlayersPerTeam { get; set; } = 11;
    public double? AssociationLatitude { get; set; }
    public double? AssociationLongitude { get; set; }
    public double? CheckinRadiusMeters { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
