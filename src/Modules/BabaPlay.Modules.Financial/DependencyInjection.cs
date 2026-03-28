using BabaPlay.Modules.Financial.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.Financial;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancialModule(this IServiceCollection services)
    {
        services.AddScoped<MembershipService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<CashEntryService>();
        services.AddScoped<ICashEntryService>(sp => sp.GetRequiredService<CashEntryService>());
        return services;
    }
}
