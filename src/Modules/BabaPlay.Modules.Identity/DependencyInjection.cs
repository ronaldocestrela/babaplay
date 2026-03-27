using BabaPlay.Modules.Identity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<RoleAdminService>();
        return services;
    }
}
