using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Fase 0: placeholder — EF Core, Identity e multi-tenant serão adicionados nas Fases 1 e 2
        return services;
    }
}
