using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace BabaPlay.Web.Services.SignalR;

public interface ISignalRChatService : IAsyncDisposable
{
    HubConnectionState State { get; }
    bool IsConnected { get; }
    event Action<HubConnectionState>? OnStateChanged;
    event Action<ChatMessageDto>? OnMessageReceived;

    Task ConnectAsync(string token, Guid tenantId, CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task<List<ChatMessageDto>> GetRecentMessagesAsync(CancellationToken cancellationToken = default);
    Task<bool> SendMessageAsync(string content, CancellationToken cancellationToken = default);
}
