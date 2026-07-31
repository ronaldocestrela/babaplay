using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace BabaPlay.Web.Services.SignalR;

public sealed class SignalRChatService : ISignalRChatService
{
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;
    private Guid _tenantId = Guid.Empty;
    private const string RestApiUrl = "api/v1/communication/chat/messages";
    private const string HubRelativeUrl = "hubs/chat";
    private const string TenantSlugHeader = "X-Tenant-Slug";

    public HubConnectionState State => _hubConnection?.State ?? HubConnectionState.Disconnected;
    public bool IsConnected => State == HubConnectionState.Connected;

    public event Action<HubConnectionState>? OnStateChanged;
    public event Action<ChatMessageDto>? OnMessageReceived;
    public event Action<string>? OnConnectionFailed;

    public SignalRChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ConnectAsync(
        string token,
        string? tenantSlug,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            return;

        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync(cancellationToken);
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }

        _tenantId = tenantId;

        var baseAddress = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "";
        var hubUrl = $"{baseAddress}/{HubRelativeUrl}";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                }

                if (!string.IsNullOrWhiteSpace(tenantSlug))
                {
                    options.Headers[TenantSlugHeader] = tenantSlug;
                }
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<ChatMessageDto>("receiveMessage", message =>
        {
            OnMessageReceived?.Invoke(message);
        });

        _hubConnection.Reconnecting += _ =>
        {
            OnStateChanged?.Invoke(HubConnectionState.Reconnecting);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += async _ =>
        {
            OnStateChanged?.Invoke(HubConnectionState.Connected);

            if (_tenantId != Guid.Empty && _hubConnection is not null)
            {
                try
                {
                    await _hubConnection.SendAsync("JoinTenantChat", _tenantId);
                }
                catch
                {
                    // Rejoin failures are non-fatal; the client may retry on the next reconnect.
                }
            }
        };

        _hubConnection.Closed += _ =>
        {
            OnStateChanged?.Invoke(HubConnectionState.Disconnected);
            return Task.CompletedTask;
        };

        OnStateChanged?.Invoke(HubConnectionState.Connecting);

        try
        {
            await _hubConnection.StartAsync(cancellationToken);
            OnStateChanged?.Invoke(HubConnectionState.Connected);

            if (_tenantId != Guid.Empty)
            {
                await _hubConnection.SendAsync("JoinTenantChat", _tenantId, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            OnConnectionFailed?.Invoke(ex.Message);
            OnStateChanged?.Invoke(HubConnectionState.Disconnected);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            OnStateChanged?.Invoke(HubConnectionState.Disconnected);
        }
    }

    public async Task<List<ChatMessageDto>> GetRecentMessagesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(RestApiUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return [];

        var messages = await response.Content.ReadFromJsonAsync<List<ChatMessageDto>>(cancellationToken: cancellationToken);
        return messages ?? [];
    }

    public async Task<bool> SendMessageAsync(string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var dto = new SendChatMessageDto(content.Trim());
        var response = await _httpClient.PostAsJsonAsync(RestApiUrl, dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
