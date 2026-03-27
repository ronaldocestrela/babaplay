using BabaPlay.Modules.TeamGeneration.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.TeamGeneration;

public static class DependencyInjection
{
    public static IServiceCollection AddTeamGenerationModule(this IServiceCollection services)
    {
        services.AddScoped<TeamGenerationService>();
        return services;
    }
}
