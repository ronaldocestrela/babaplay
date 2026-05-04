using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Commands.Checkins;
using BabaPlay.Application.Commands.GameDays;
using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.Commands.MatchEvents;
using BabaPlay.Application.Commands.Ping;
using BabaPlay.Application.Commands.Players;
using BabaPlay.Application.Commands.Positions;
using BabaPlay.Application.Commands.Roles;
using BabaPlay.Application.Commands.Teams;
using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Ping;
using BabaPlay.Application.Queries.Checkins;
using BabaPlay.Application.Queries.GameDays;
using BabaPlay.Application.Queries.Matches;
using BabaPlay.Application.Queries.MatchEvents;
using BabaPlay.Application.Queries.Players;
using BabaPlay.Application.Queries.Positions;
using BabaPlay.Application.Queries.Roles;
using BabaPlay.Application.Queries.Teams;
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

        // GameDays — Fase 6
        services.AddScoped<ICommandHandler<CreateGameDayCommand, Result<GameDayResponse>>, CreateGameDayCommandHandler>();
        services.AddScoped<IQueryHandler<GetGameDayQuery, Result<GameDayResponse>>, GetGameDayQueryHandler>();
        services.AddScoped<IQueryHandler<GetGameDaysQuery, Result<IReadOnlyList<GameDayResponse>>>, GetGameDaysQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateGameDayCommand, Result<GameDayResponse>>, UpdateGameDayCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeGameDayStatusCommand, Result<GameDayResponse>>, ChangeGameDayStatusCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteGameDayCommand, Result>, DeleteGameDayCommandHandler>();

        // Matches — Fase 9
        services.AddScoped<ICommandHandler<CreateMatchCommand, Result<MatchResponse>>, CreateMatchCommandHandler>();
        services.AddScoped<IQueryHandler<GetMatchQuery, Result<MatchResponse>>, GetMatchQueryHandler>();
        services.AddScoped<IQueryHandler<GetMatchesQuery, Result<IReadOnlyList<MatchResponse>>>, GetMatchesQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateMatchCommand, Result<MatchResponse>>, UpdateMatchCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeMatchStatusCommand, Result<MatchResponse>>, ChangeMatchStatusCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteMatchCommand, Result>, DeleteMatchCommandHandler>();

        // MatchEvents — Fase 10
        services.AddScoped<ICommandHandler<CreateMatchEventTypeCommand, Result<MatchEventTypeResponse>>, CreateMatchEventTypeCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMatchEventTypeCommand, Result<MatchEventTypeResponse>>, UpdateMatchEventTypeCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteMatchEventTypeCommand, Result>, DeleteMatchEventTypeCommandHandler>();
        services.AddScoped<IQueryHandler<GetMatchEventTypeQuery, Result<MatchEventTypeResponse>>, GetMatchEventTypeQueryHandler>();
        services.AddScoped<IQueryHandler<GetMatchEventTypesQuery, Result<IReadOnlyList<MatchEventTypeResponse>>>, GetMatchEventTypesQueryHandler>();
        services.AddScoped<ICommandHandler<CreateMatchEventCommand, Result<MatchEventResponse>>, CreateMatchEventCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMatchEventCommand, Result<MatchEventResponse>>, UpdateMatchEventCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteMatchEventCommand, Result>, DeleteMatchEventCommandHandler>();
        services.AddScoped<IQueryHandler<GetMatchEventQuery, Result<MatchEventResponse>>, GetMatchEventQueryHandler>();
        services.AddScoped<IQueryHandler<GetMatchEventsByMatchQuery, Result<IReadOnlyList<MatchEventResponse>>>, GetMatchEventsByMatchQueryHandler>();
        services.AddScoped<IQueryHandler<GetMatchEventsByPlayerQuery, Result<IReadOnlyList<MatchEventResponse>>>, GetMatchEventsByPlayerQueryHandler>();

        // Players — Fase 3
        services.AddScoped<ICommandHandler<CreatePlayerCommand, Result<PlayerResponse>>, CreatePlayerCommandHandler>();
        services.AddScoped<IQueryHandler<GetPlayerQuery, Result<PlayerResponse>>, GetPlayerQueryHandler>();
        services.AddScoped<IQueryHandler<GetPlayersQuery, Result<IReadOnlyList<PlayerResponse>>>, GetPlayersQueryHandler>();
        services.AddScoped<ICommandHandler<UpdatePlayerCommand, Result<PlayerResponse>>, UpdatePlayerCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePlayerCommand, Result>, DeletePlayerCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePlayerPositionsCommand, Result<PlayerPositionsResponse>>, UpdatePlayerPositionsCommandHandler>();

        // Positions — Fase 5
        services.AddScoped<ICommandHandler<CreatePositionCommand, Result<PositionResponse>>, CreatePositionCommandHandler>();
        services.AddScoped<IQueryHandler<GetPositionQuery, Result<PositionResponse>>, GetPositionQueryHandler>();
        services.AddScoped<IQueryHandler<GetPositionsQuery, Result<IReadOnlyList<PositionResponse>>>, GetPositionsQueryHandler>();
        services.AddScoped<ICommandHandler<UpdatePositionCommand, Result<PositionResponse>>, UpdatePositionCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePositionCommand, Result>, DeletePositionCommandHandler>();

        // RBAC — Fase 4
        services.AddScoped<ICommandHandler<CreateRoleCommand, Result<RoleResponse>>, CreateRoleCommandHandler>();
        services.AddScoped<IQueryHandler<GetRolesQuery, Result<IReadOnlyList<RoleResponse>>>, GetRolesQueryHandler>();
        services.AddScoped<ICommandHandler<AssignRoleToUserCommand, Result>, AssignRoleToUserCommandHandler>();
        services.AddScoped<ICommandHandler<AddPermissionToRoleCommand, Result<RoleResponse>>, AddPermissionToRoleCommandHandler>();

        // Check-ins — Fase 7
        services.AddScoped<ICommandHandler<CreateCheckinCommand, Result<CheckinResponse>>, CreateCheckinCommandHandler>();
        services.AddScoped<ICommandHandler<CancelCheckinCommand, Result>, CancelCheckinCommandHandler>();
        services.AddScoped<IQueryHandler<GetCheckinsByGameDayQuery, Result<IReadOnlyList<CheckinResponse>>>, GetCheckinsByGameDayQueryHandler>();
        services.AddScoped<IQueryHandler<GetCheckinsByPlayerQuery, Result<IReadOnlyList<CheckinResponse>>>, GetCheckinsByPlayerQueryHandler>();

        // Teams — Fase 8
        services.AddScoped<ICommandHandler<CreateTeamCommand, Result<TeamResponse>>, CreateTeamCommandHandler>();
        services.AddScoped<IQueryHandler<GetTeamQuery, Result<TeamResponse>>, GetTeamQueryHandler>();
        services.AddScoped<IQueryHandler<GetTeamsQuery, Result<IReadOnlyList<TeamResponse>>>, GetTeamsQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateTeamCommand, Result<TeamResponse>>, UpdateTeamCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTeamPlayersCommand, Result<TeamPlayersResponse>>, UpdateTeamPlayersCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTeamCommand, Result>, DeleteTeamCommandHandler>();

        return services;
    }
}

