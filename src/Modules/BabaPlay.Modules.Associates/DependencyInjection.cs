using BabaPlay.Modules.Associates.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Modules.Associates;

public static class DependencyInjection
{
    public static IServiceCollection AddAssociatesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<InvitationLinkOptions>()
            .Bind(configuration.GetSection(InvitationLinkOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.FrontendBaseUrl),
                "Invitations:FrontendBaseUrl must be configured.")
            .ValidateOnStart();

        services.AddScoped<AssociateService>();
        services.AddScoped<BabaPlay.SharedKernel.Security.IAssociateInvitationService, AssociateInvitationService>();
        services.AddScoped<PositionService>();
        return services;
    }
}
