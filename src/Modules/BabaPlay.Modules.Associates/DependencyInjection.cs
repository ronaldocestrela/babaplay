using BabaPlay.Modules.Associates.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.Associates;

public static class DependencyInjection
{
    public static IServiceCollection AddAssociatesModule(this IServiceCollection services)
    {
        services.AddScoped<AssociateService>();
        services.AddScoped<PositionService>();
        return services;
    }
}
