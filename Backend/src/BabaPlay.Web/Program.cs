using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BabaPlay.Web;
using BabaPlay.Web.Services.Handlers;
using BabaPlay.Web.Services.State;
using BabaPlay.Web.Services.Storage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<IAuthSessionStorage, LocalStorageAuthSessionStorage>();
builder.Services.AddScoped<AuthSessionService>();

// State Providers (Scoped)
builder.Services.AddScoped<TenantState>();
builder.Services.AddScoped<UserSessionState>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// HTTP Handler & Client
// Do NOT use IHttpClientFactory for this client: it builds handlers in a separate DI scope,
// so AuthorizationHeaderHandler would receive a different (empty) UserSessionState/TenantState
// and every API call would go out without Bearer / X-Tenant-Slug → 401/403.
builder.Services.AddScoped<AuthorizationHeaderHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationHeaderHandler>();
    handler.InnerHandler = new HttpClientHandler();
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl),
    };
});

// API Services
builder.Services.AddScoped<BabaPlay.Web.Services.Http.IAuthApiService, BabaPlay.Web.Services.Http.AuthApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.ITenantApiService, BabaPlay.Web.Services.Http.TenantApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.IPlayerApiService, BabaPlay.Web.Services.Http.PlayerApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.IPositionApiService, BabaPlay.Web.Services.Http.PositionApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.IDashboardApiService, BabaPlay.Web.Services.Http.DashboardApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.IMatchApiService, BabaPlay.Web.Services.Http.MatchApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.ICheckinApiService, BabaPlay.Web.Services.Http.CheckinApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.ITeamApiService, BabaPlay.Web.Services.Http.TeamApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.IRankingApiService, BabaPlay.Web.Services.Http.RankingApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.IFinancialApiService, BabaPlay.Web.Services.Http.FinancialApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.ICommunicationApiService, BabaPlay.Web.Services.Http.CommunicationApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.Http.INotificationApiService, BabaPlay.Web.Services.Http.NotificationApiService>();
builder.Services.AddScoped<BabaPlay.Web.Services.SignalR.ISignalRChatService, BabaPlay.Web.Services.SignalR.SignalRChatService>();







await builder.Build().RunAsync();
