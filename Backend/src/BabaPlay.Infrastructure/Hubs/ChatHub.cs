using BabaPlay.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BabaPlay.Infrastructure.Hubs;

[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class ChatHub : Hub
{
    public const string ReceiveMessageEvent = "receiveMessage";
    public const string UserTypingEvent = "userTyping";

    public static string TenantGroup(Guid tenantId) => $"tenant:{tenantId}";

    public Task JoinTenantChat(Guid tenantId)
        => Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(tenantId));

    public Task LeaveTenantChat(Guid tenantId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, TenantGroup(tenantId));

    public async Task SendMessage(Guid tenantId, string senderName, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        var payload = new
        {
            id = Guid.NewGuid(),
            tenantId,
            senderId = Context.UserIdentifier,
            senderName,
            content = content.Trim(),
            sentAtUtc = DateTime.UtcNow
        };

        await Clients.Group(TenantGroup(tenantId)).SendAsync(ReceiveMessageEvent, payload);
    }
}
