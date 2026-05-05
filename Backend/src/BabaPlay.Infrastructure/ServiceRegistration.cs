using System.Text;
using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Authorization;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using BabaPlay.Infrastructure.Repositories;
using BabaPlay.Infrastructure.Services;
using BabaPlay.Infrastructure.Settings;
using BabaPlay.Infrastructure.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BabaPlay.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- JWT Settings ---
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection);
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException($"'{JwtSettings.SectionName}' configuration section is missing.");

        var matchSummaryStorageSection = configuration.GetSection(MatchSummaryStorageSettings.SectionName);
        services.Configure<MatchSummaryStorageSettings>(matchSummaryStorageSection);

        // --- Master Database ---
        services.AddDbContext<MasterDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("MasterDb")
                ?? throw new InvalidOperationException("'ConnectionStrings:MasterDb' is missing.")));

        // --- ASP.NET Identity ---
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<MasterDbContext>()
        .AddDefaultTokenProviders();

        // --- JWT Bearer Authentication ---
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero,
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicyNames.TenantMember, policy =>
                policy.Requirements.Add(new TenantMemberRequirement()));

            options.AddPolicy(AuthorizationPolicyNames.RbacRolesRead, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.RbacRolesRead));
            });

            options.AddPolicy(AuthorizationPolicyNames.RbacRolesWrite, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.RbacRolesWrite));
            });

            options.AddPolicy(AuthorizationPolicyNames.RbacRolesAssign, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.RbacRolesAssign));
            });

            options.AddPolicy(AuthorizationPolicyNames.RbacPermissionsWrite, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.RbacPermissionsWrite));
            });

            options.AddPolicy(AuthorizationPolicyNames.MatchesRead, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.MatchesRead));
            });

            options.AddPolicy(AuthorizationPolicyNames.MatchesWrite, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.MatchesWrite));
            });

            options.AddPolicy(AuthorizationPolicyNames.MatchEventsRead, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.MatchEventsRead));
            });

            options.AddPolicy(AuthorizationPolicyNames.MatchEventsWrite, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.MatchEventsWrite));
            });

            options.AddPolicy(AuthorizationPolicyNames.MatchEventTypesRead, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.MatchEventTypesRead));
            });

            options.AddPolicy(AuthorizationPolicyNames.MatchEventTypesWrite, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.MatchEventTypesWrite));
            });

            options.AddPolicy(AuthorizationPolicyNames.RankingRead, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.RankingRead));
            });

            options.AddPolicy(AuthorizationPolicyNames.RankingWrite, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.RankingWrite));
            });

            options.AddPolicy(AuthorizationPolicyNames.FinancialRead, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.FinancialRead));
            });

            options.AddPolicy(AuthorizationPolicyNames.FinancialWrite, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.FinancialWrite));
            });

            options.AddPolicy(AuthorizationPolicyNames.FinancialApprove, policy =>
            {
                policy.Requirements.Add(new TenantMemberRequirement());
                policy.Requirements.Add(new PermissionRequirement(RbacCatalog.Permissions.FinancialApprove));
            });
        });

        services.AddScoped<IAuthorizationHandler, TenantMemberAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // --- Application-level service abstractions ---
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // --- Multi-tenancy (Fase 2) ---
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, RequestTenantContext>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserTenantRepository, UserTenantRepository>();
        services.AddScoped<ITenantOwnerProvisioningService, TenantOwnerProvisioningService>();
        services.AddScoped<TenantDbContextFactory>();
        services.AddSingleton<ITenantProvisioningQueue, TenantProvisioningQueue>();
        services.AddHostedService<TenantProvisioningWorker>();

        // --- Tenant-scoped repositories (Fase 3) ---
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IMatchSummaryRepository, MatchSummaryRepository>();
        services.AddScoped<IMatchEventRepository, MatchEventRepository>();
        services.AddScoped<IMatchEventTypeRepository, MatchEventTypeRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IGameDayRepository, GameDayRepository>();
        services.AddScoped<ICheckinRepository, CheckinRepository>();
        services.AddScoped<IPlayerScoreRepository, PlayerScoreRepository>();
        services.AddScoped<ICashTransactionRepository, CashTransactionRepository>();
        services.AddScoped<IPlayerMonthlyFeeRepository, PlayerMonthlyFeeRepository>();
        services.AddScoped<IMonthlyFeePaymentRepository, MonthlyFeePaymentRepository>();
        services.AddScoped<ITenantGeolocationSettingsRepository, TenantGeolocationSettingsRepository>();
        services.AddScoped<ICheckinRealtimeNotifier, SignalRCheckinRealtimeNotifier>();
        services.AddScoped<IMatchEventRealtimeNotifier, SignalRMatchEventRealtimeNotifier>();
        services.AddScoped<IMatchSummaryPdfGenerator, MinimalPdfMatchSummaryGenerator>();
        services.AddScoped<IMatchSummaryStorageService, LocalMatchSummaryStorageService>();

        // --- RBAC repositories (Fase 4) ---
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        return services;
    }
}

