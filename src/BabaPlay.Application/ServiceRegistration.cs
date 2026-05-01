using BabaPlay.Application.Commands.Ping;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Ping;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<PingCommand, Result<string>>, PingCommandHandler>();
        services.AddScoped<IQueryHandler<PingQuery, Result<PingStatusDto>>, PingQueryHandler>();

        return services;
    }
}
