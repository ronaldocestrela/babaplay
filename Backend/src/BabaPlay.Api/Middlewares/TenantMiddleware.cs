using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Api.Middlewares;

/// <summary>
/// Reads the <c>X-Tenant-Slug</c> header (set by the reverse proxy or the frontend),
/// validates the tenant against the Master DB, and stores the resolved tenant info
/// in the scoped <see cref="ITenantContext"/> for the duration of the request.
///
/// Behaviour:
/// - Header absent  → tenant is not resolved (<see cref="ITenantContext.IsResolved"/> = false);
///   request continues normally (auth routes do not require a tenant).
/// - Header present, tenant found and active → resolves context.
/// - Header present, tenant not found or inactive → 404 TENANT_NOT_FOUND.
/// </summary>
public sealed class TenantMiddleware
{
    public const string HeaderName = "X-Tenant-Slug";

    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        ITenantRepository tenantRepository,
        ITenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var slugValues))
        {
            var slug = slugValues.FirstOrDefault()?.Trim().ToLowerInvariant();

            if (!string.IsNullOrEmpty(slug))
            {
                var tenant = await tenantRepository.GetBySlugAsync(slug, context.RequestAborted);

                if (tenant is null || !tenant.IsActive)
                    throw new NotFoundException("TENANT_NOT_FOUND", $"Tenant '{slug}' was not found or is inactive.");

                tenantContext.Set(tenant.Id, tenant.Slug);
            }
        }

        await _next(context);
    }
}

/// <summary>Extension method for clean registration in Program.cs.</summary>
public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantMiddleware>();
}
