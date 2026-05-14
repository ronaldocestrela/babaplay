using BabaPlay.Api.Middlewares;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BabaPlay.Api.Filters;

/// <summary>
/// Adiciona o parâmetro X-Tenant-Slug como header opcional em todas as operações do Swagger.
/// </summary>
public sealed class TenantSlugHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = TenantMiddleware.HeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema { Type = "string" },
            Description = "Slug do tenant. Obrigatório para rotas que acessam dados do tenant.",
        });
    }
}
