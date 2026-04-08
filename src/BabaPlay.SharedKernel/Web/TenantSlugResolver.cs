using Microsoft.AspNetCore.Http;

namespace BabaPlay.SharedKernel.Web;

/// <summary>
/// Resolves the tenant slug for multitenant requests. Order: <c>X-Tenant-Subdomain</c> header,
/// then query <c>tenant</c> (for single-domain / SPA deployments), then the first host label when
/// the host has multiple segments (e.g. <c>club.example.com</c>).
/// </summary>
public static class TenantSlugResolver
{
    public const string TenantQueryParameterName = "tenant";

    public static string? Resolve(HttpRequest request)
    {
        string? header = null;
        if (request.Headers.TryGetValue("X-Tenant-Subdomain", out var hv) && !string.IsNullOrWhiteSpace(hv))
            header = hv.ToString().Trim();

        string? queryTenant = null;
        if (request.Query.TryGetValue(TenantQueryParameterName, out var qv) && !string.IsNullOrWhiteSpace(qv))
            queryTenant = qv.ToString().Trim();

        return Resolve(header, queryTenant, request.Host.Host);
    }

    /// <summary>Test helper: same resolution as <see cref="Resolve(HttpRequest)"/> without HTTP context.</summary>
    public static string? Resolve(string? headerSubdomain, string? queryTenant, string host)
    {
        if (!string.IsNullOrWhiteSpace(headerSubdomain))
            return headerSubdomain.Trim();

        if (!string.IsNullOrWhiteSpace(queryTenant))
            return queryTenant.Trim();

        return ResolveFromHost(host);
    }

    private static string? ResolveFromHost(string host)
    {
        if (string.IsNullOrEmpty(host))
            return null;

        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2)
        {
            var sub = parts[0];
            if (!sub.Equals("www", StringComparison.OrdinalIgnoreCase))
                return sub;
        }

        return null;
    }
}
