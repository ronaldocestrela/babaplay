using BabaPlay.Infrastructure;
using BabaPlay.Infrastructure.Multitenancy;
using BabaPlay.Infrastructure.Persistence;
using BabaPlay.Modules.Associates;
using BabaPlay.Modules.Associations;
using BabaPlay.Modules.CheckIns;
using BabaPlay.Modules.Financial;
using BabaPlay.Modules.Identity;
using BabaPlay.Modules.Platform;
using BabaPlay.Modules.Platform.Entities;
using BabaPlay.Modules.TeamGeneration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPlatformModule();
builder.Services.AddIdentityModule();
builder.Services.AddAssociationsModule();
builder.Services.AddAssociatesModule(builder.Configuration);
builder.Services.AddCheckInsModule();
builder.Services.AddTeamGenerationModule();
builder.Services.AddFinancialModule();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(BabaPlay.Modules.Platform.Controllers.PlansController).Assembly)
    .AddApplicationPart(typeof(BabaPlay.Modules.Identity.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(BabaPlay.Modules.Associations.Controllers.AssociationsController).Assembly)
    .AddApplicationPart(typeof(BabaPlay.Modules.Associates.Controllers.AssociatesController).Assembly)
    .AddApplicationPart(typeof(BabaPlay.Modules.CheckIns.Controllers.CheckInsController).Assembly)
    .AddApplicationPart(typeof(BabaPlay.Modules.TeamGeneration.Controllers.TeamsController).Assembly)
    .AddApplicationPart(typeof(BabaPlay.Modules.Financial.Controllers.MembershipsController).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.CustomSchemaIds(t => t.FullName?.Replace("+", ".", StringComparison.Ordinal));
    o.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT token"
    });
    o.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await platform.Database.MigrateAsync();

    if (!await platform.Plans.AnyAsync())
    {
        platform.Plans.Add(new Plan
        {
            Name = "Starter",
            Description = "Default MVP plan",
            MonthlyPrice = 0,
            MaxAssociates = 200
        });
        await platform.SaveChangesAsync();
    }

    if (!await platform.AllowedOrigins.AnyAsync())
    {
        platform.AllowedOrigins.Add(new AllowedOrigin { Origin = "http://localhost:5173" });
        platform.AllowedOrigins.Add(new AllowedOrigin { Origin = "http://localhost:3000" });
        await platform.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Dynamic");
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
