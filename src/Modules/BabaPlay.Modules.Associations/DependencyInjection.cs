using BabaPlay.Modules.Associations.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.Associations;

public static class DependencyInjection
{
    public static IServiceCollection AddAssociationsModule(this IServiceCollection services)
    {
        services.AddScoped<AssociationService>();
        return services;
    }
}
