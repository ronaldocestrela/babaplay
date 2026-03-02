using Infrastructure;
using Application;
using WebApi;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Infrastructure.Cors;
using Application.Features.Cors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Services.GetJwtSettings(builder.Configuration));

builder.Services.AddApplicationServices();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICorsPolicyProvider, DynamicCorsPolicyProvider>();

// Dynamic CORS provider is used for all origins.  
// Legacy development-only policy removed in favor of storing allowed
// origins in the database or cache at startup if needed.
// builder.Services.AddCors(o => o.AddPolicy("AllowBlazor",
//     p => p.WithOrigins("http://localhost:5145").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

await app.Services.AddDatabaseInitializerAsync();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

app.UseHttpsRedirection();

// CORS must run before authentication/authorization and before mapping
// controllers so the policy is applied to every request.  The empty
// parameter causes the framework to resolve the policy via the
// registered ICorsPolicyProvider (our DynamicCorsPolicyProvider).
app.UseCors();

app.UseInfrastructure();

// diagnostics middleware (development only) — logs auth/tenant info for incoming requests
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
        .CreateLogger("RequestDiagnostics");

    var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
    var tenantClaim = context.User?.Claims?.FirstOrDefault(c => c.Type == "tenant")?.Value ?? string.Empty;
    var tenantHeader = context.Request.Headers.ContainsKey("tenant") ? context.Request.Headers["tenant"].ToString() : string.Empty;

    logger.LogInformation("Incoming request {Method} {Path} - Authenticated={Authenticated} TenantClaim={TenantClaim} TenantHeader={TenantHeader}",
        context.Request.Method, context.Request.Path, isAuthenticated, tenantClaim, tenantHeader);

    if (isAuthenticated)
    {
        var claims = string.Join(',', context.User!.Claims.Select(c => $"{c.Type}={c.Value}"));
        logger.LogInformation("User claims: {Claims}", claims);
    }

    await next();
});

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
