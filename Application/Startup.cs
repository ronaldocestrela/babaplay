using Application.Pipelines;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;

public static class Startup
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assmbly = Assembly.GetExecutingAssembly();

        return services
            .AddValidatorsFromAssembly(assmbly)
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBenaviour<,>))
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assmbly);
            });
    }
}
