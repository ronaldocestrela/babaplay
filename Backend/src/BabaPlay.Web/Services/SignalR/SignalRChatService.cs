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
    private const string RestApiUrl = "api/v1/communication/chat/messages";
    private const string HubRelativeUrl = "hubs/chat";

    public HubConnectionState State => _hubConnection?.State ?? HubConnectionState.Disconnected;
    public bool IsConnected => State == HubConnectionState.Connected;

    public event Action<HubConnectionState>? OnStateChanged;
    public event Action<ChatMessageDto>? OnMessageReceived;

    public SignalRChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ConnectAsync(string token, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            return;

        var baseAddress = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "";
        var hubUrl = $"{baseAddress}/{HubRelativeUrl}";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
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

        _hubConnection.Reconnected += _ =>
        {
            OnStateChanged?.Invoke(HubConnectionState.Connected);
            return Task.CompletedTask;
        };

        _hubConnection.Closed += _ =>
        {
            OnStateChanged?.Invoke(HubConnectionState.Disconnected);
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync(cancellationToken);
            OnStateChanged?.Invoke(HubConnectionState.Connected);

            // Entra no grupo do tenant
            if (tenantId != Guid.Empty)
            {
                await _hubConnection.SendAsync("JoinTenantChat", tenantId, cancellationToken);
            }
        }
        catch
        {
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
