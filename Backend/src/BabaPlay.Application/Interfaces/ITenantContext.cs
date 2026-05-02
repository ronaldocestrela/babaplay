namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Provides access to the tenant resolved for the current HTTP request.
/// Populated by TenantMiddleware; consumed by handlers and services.
/// </summary>
public interface ITenantContext
{
    /// <summary>Resolved tenant identifier (empty Guid if not resolved).</summary>
    Guid TenantId { get; }

    /// <summary>Resolved URL-safe slug (empty string if not resolved).</summary>
    string TenantSlug { get; }

    /// <summary>True when a tenant header was present and validated.</summary>
    bool IsResolved { get; }

    /// <summary>Called by TenantMiddleware to store resolved values for this request.</summary>
    void Set(Guid tenantId, string tenantSlug);
}
