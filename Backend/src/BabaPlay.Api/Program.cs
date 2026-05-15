using BabaPlay.Api.Filters;
using BabaPlay.Api.Middlewares;
using BabaPlay.Application;
using BabaPlay.Infrastructure;
using BabaPlay.Infrastructure.Hubs;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// CORS: permite frontend local em desenvolvimento
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        var allowedOrigins = (builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:5173"])
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        static string NormalizeOrigin(string origin)
            => origin.Trim().TrimEnd('/');

        policy
            .SetIsOriginAllowed(origin => allowedOrigins.Contains(NormalizeOrigin(origin)))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "BabaPlay API",
        Version = "v1",
        Description = "SaaS backend para gestão de associações esportivas"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT: Bearer {token}",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    options.OperationFilter<TenantSlugHeaderOperationFilter>();

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// --- Pipeline ---
var app = builder.Build();

// Applies pending EF Core migrations for the master database on startup.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
    masterDb.Database.Migrate();
}

app.UseExceptionHandler();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BabaPlay API v1"));
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseTenantResolution();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<CheckinHub>("/hubs/checkin");
app.MapHub<MatchHub>("/hubs/match");

app.Run();

// Partial class para permitir acesso em testes de integração (WebApplicationFactory<Program>)
public partial class Program { }

