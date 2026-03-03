using Application.Features.Cors;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Cors;

public class DynamicCorsPolicyProvider(IServiceProvider serviceProvider) : ICorsPolicyProvider
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        using var scope = _serviceProvider.CreateScope(); 
        var corsService = scope.ServiceProvider
            .GetRequiredService<ICorsOriginService>();
            
        var origins = await corsService.GetAllowedOriginsAsync();
        var builder = new CorsPolicyBuilder();
        // allow wildcard subdomains (e.g. https://*.babaplay.com)
        builder.SetIsOriginAllowedToAllowWildcardSubdomains();

        if (origins.Any())
        { 
            builder.WithOrigins(origins.ToArray())
                .AllowAnyHeader() 
                .AllowAnyMethod() 
                .AllowCredentials(); 
        }
        
        return builder.Build();
    }
}
