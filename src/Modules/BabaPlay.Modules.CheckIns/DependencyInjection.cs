using BabaPlay.Modules.CheckIns.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.CheckIns;

public static class DependencyInjection
{
    public static IServiceCollection AddCheckInsModule(this IServiceCollection services)
    {
        services.AddScoped<CheckInService>();
        return services;
    }
}
