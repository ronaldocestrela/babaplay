namespace BabaPlay.Infrastructure.Multitenancy;

public interface ITenantProvider
{
    /// <summary>Resolved tenant id from platform DB, if any.</summary>
    string? TenantId { get; }

    /// <summary>SQL connection string for the current tenant database.</summary>
    string? TenantConnectionString { get; }

    /// <summary>True when request is for platform (backoffice) without tenant context.</summary>
    bool IsPlatformRequest { get; }

    void SetPlatformContext();
    void SetTenant(string tenantId, string connectionString);
    void Clear();
}
