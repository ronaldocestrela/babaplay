using BabaPlay.Infrastructure.Persistence;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BabaPlay.Infrastructure.Multitenancy;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, IServiceProvider services)
    {
        tenantProvider.Clear();

        var path = context.Request.Path.Value ?? string.Empty;
        if (IsPlatformPath(path))
        {
            tenantProvider.SetPlatformContext();
            await _next(context);
            return;
        }

        var slug = TenantSlugResolver.Resolve(context.Request);
        if (string.IsNullOrWhiteSpace(slug))
        {
            _logger.LogWarning("Tenant slug missing for {Path}", path);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Tenant not specified (X-Tenant-Subdomain header, tenant query parameter, or tenant subdomain in host)."
            });
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var tenant = await platform.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Subdomain == slug);
        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not found." });
            return;
        }

        var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseOptions>>().Value;
        var cs = BuildTenantConnectionString(options.PlatformConnectionString, tenant.DatabaseName);
        tenantProvider.SetTenant(tenant.Id, cs);

        await _next(context);
    }

    private static bool IsPlatformPath(string path) =>
        path.StartsWith("/api/platform", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase);

    private static string BuildTenantConnectionString(string platformConnectionString, string databaseName)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(platformConnectionString)
        {
            InitialCatalog = databaseName
        };
        return builder.ConnectionString;
    }
}
