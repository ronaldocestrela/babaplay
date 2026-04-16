using BabaPlay.Modules.MatchReports.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.MatchReports;

public static class DependencyInjection
{
    public static IServiceCollection AddMatchReportsModule(this IServiceCollection services)
    {
        services.AddScoped<MatchReportService>();
        return services;
    }
}