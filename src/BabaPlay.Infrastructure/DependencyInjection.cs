using BabaPlay.Infrastructure.Multitenancy;
using BabaPlay.Infrastructure.Messaging;
using BabaPlay.Infrastructure.Persistence;
using BabaPlay.Infrastructure.Security;
using BabaPlay.Modules.Identity;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Security;
using BabaPlay.SharedKernel.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BabaPlay.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        services.AddSingleton<ITenantProvider, TenantProvider>();
        services.AddSingleton<AllowedOriginsCache>();
        services.AddHostedService<AllowedOriginsSyncWorker>();
        services.AddSingleton<ITenantDatabaseMigrator, TenantDatabaseMigrator>();
        services.AddSingleton<TenantMigrationOrchestrator>();
        services.AddHostedService<TenantMigrationsHostedService>();
        services.AddSingleton<ICorsPolicyProvider, DynamicCorsPolicyProvider>();

        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
                        ?? new DatabaseOptions();

        services.AddDbContext<PlatformDbContext>(o => o.UseSqlServer(dbOptions.PlatformConnectionString));

        services.AddDbContext<TenantDbContext>((sp, o) =>
        {
            var tenant = sp.GetRequiredService<ITenantProvider>();
            var cs = tenant.TenantConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Tenant connection string not resolved for this request.");
            o.UseSqlServer(cs);
        });

        services
            .AddIdentityCore<ApplicationUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.Password.RequiredLength = 6;
                o.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<TenantDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddScoped(typeof(IPlatformRepository<>), typeof(EfPlatformRepository<>));
        services.AddScoped(typeof(ITenantRepository<>), typeof(EfTenantRepository<>));
        services.AddScoped<IPlatformUnitOfWork>(sp => sp.GetRequiredService<PlatformDbContext>());
        services.AddScoped<ITenantUnitOfWork>(sp => sp.GetRequiredService<TenantDbContext>());
        services.AddScoped<TenantDatabaseProvisioner>();
        services.AddScoped<SharedKernel.Services.ITenantProvisioningService>(sp => sp.GetRequiredService<TenantDatabaseProvisioner>());
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddScoped<IPermissionResolver, PermissionResolver>();
        services.AddScoped<IAssociateStatusChecker, AssociateStatusChecker>();
        services.AddScoped<IAssociateUserProvisioner, AssociateUserProvisioner>();
        services.AddScoped<IAssociateSignupSynchronizer, AssociateSignupSynchronizer>();
        services.AddHttpClient<ResendEmailService>(client =>
        {
            client.BaseAddress = new Uri("https://api.resend.com/");
        });
        services.AddScoped<IEmailService>(sp => sp.GetRequiredService<ResendEmailService>());

        services.AddCors();
        return services;
    }
}
