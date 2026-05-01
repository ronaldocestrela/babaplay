using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Commands.Ping;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Ping;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Ping
        services.AddScoped<ICommandHandler<PingCommand, Result<string>>, PingCommandHandler>();
        services.AddScoped<IQueryHandler<PingQuery, Result<PingStatusDto>>, PingQueryHandler>();

        // Auth — Fase 1
        services.AddScoped<ICommandHandler<LoginCommand, Result<AuthResponse>>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, Result<AuthResponse>>, RefreshTokenCommandHandler>();

        return services;
    }
}
