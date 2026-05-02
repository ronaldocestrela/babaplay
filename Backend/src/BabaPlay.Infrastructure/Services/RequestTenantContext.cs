using BabaPlay.Application.Interfaces;

namespace BabaPlay.Infrastructure.Services;

/// <summary>
/// Scoped tenant context for the current HTTP request.
/// Set by <c>TenantMiddleware</c>; read by application handlers.
/// </summary>
public sealed class RequestTenantContext : ITenantContext
{
    private Guid _tenantId;
    private string _tenantSlug = string.Empty;

    /// <inheritdoc />
    public Guid TenantId => _tenantId;

    /// <inheritdoc />
    public string TenantSlug => _tenantSlug;

    /// <inheritdoc />
    public bool IsResolved { get; private set; }

    /// <inheritdoc />
    public void Set(Guid tenantId, string tenantSlug)
    {
        _tenantId = tenantId;
        _tenantSlug = tenantSlug;
        IsResolved = true;
    }
}
