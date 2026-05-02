using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Commands.Ping;
using BabaPlay.Application.Commands.Players;
using BabaPlay.Application.Commands.Roles;
using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Ping;
using BabaPlay.Application.Queries.Players;
using BabaPlay.Application.Queries.Roles;
using BabaPlay.Application.Queries.Tenants;
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

        // Tenants — Fase 2
        services.AddScoped<ICommandHandler<CreateTenantCommand, Result<TenantResponse>>, CreateTenantCommandHandler>();
        services.AddScoped<IQueryHandler<GetTenantStatusQuery, Result<TenantResponse>>, GetTenantStatusQueryHandler>();

        // Players — Fase 3
        services.AddScoped<ICommandHandler<CreatePlayerCommand, Result<PlayerResponse>>, CreatePlayerCommandHandler>();
        services.AddScoped<IQueryHandler<GetPlayerQuery, Result<PlayerResponse>>, GetPlayerQueryHandler>();
        services.AddScoped<IQueryHandler<GetPlayersQuery, Result<IReadOnlyList<PlayerResponse>>>, GetPlayersQueryHandler>();
        services.AddScoped<ICommandHandler<UpdatePlayerCommand, Result<PlayerResponse>>, UpdatePlayerCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePlayerCommand, Result>, DeletePlayerCommandHandler>();

        // RBAC — Fase 4
        services.AddScoped<ICommandHandler<CreateRoleCommand, Result<RoleResponse>>, CreateRoleCommandHandler>();
        services.AddScoped<IQueryHandler<GetRolesQuery, Result<IReadOnlyList<RoleResponse>>>, GetRolesQueryHandler>();
        services.AddScoped<ICommandHandler<AssignRoleToUserCommand, Result>, AssignRoleToUserCommandHandler>();
        services.AddScoped<ICommandHandler<AddPermissionToRoleCommand, Result<RoleResponse>>, AddPermissionToRoleCommandHandler>();

        return services;
    }
}

