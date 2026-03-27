using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Infrastructure.Security;

public sealed class DynamicCorsPolicyProvider : ICorsPolicyProvider
{
    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        if (policyName is null || !string.Equals(policyName, "Dynamic", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<CorsPolicy?>(null);

        var cache = context.RequestServices.GetRequiredService<AllowedOriginsCache>();
        var builder = new CorsPolicyBuilder();
        builder.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin)) return false;
            if (origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
                origin.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            return cache.Contains(origin);
        });
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        builder.WithExposedHeaders("*");
        builder.AllowCredentials();
        return Task.FromResult<CorsPolicy?>(builder.Build());
    }
}
