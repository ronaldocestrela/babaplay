using BabaPlay.Modules.Platform.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.Platform;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformModule(this IServiceCollection services)
    {
        services.AddScoped<PlanService>();
        services.AddScoped<TenantSubscriptionService>();
        return services;
    }
}
