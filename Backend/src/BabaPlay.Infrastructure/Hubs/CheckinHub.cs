using BabaPlay.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BabaPlay.Infrastructure.Hubs;

[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class CheckinHub : Hub
{
    public const string CheckinCreatedEvent = "checkinCreated";
    public const string CheckinCountUpdatedEvent = "checkinCountUpdated";
    public const string CheckinDeniedEvent = "checkinDenied";
    public const string CheckinUndoneEvent = "checkinUndone";

    public static string GameDayGroup(Guid gameDayId) => $"gameday:{gameDayId}";

    public Task JoinGameDay(Guid gameDayId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GameDayGroup(gameDayId));

    public Task LeaveGameDay(Guid gameDayId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GameDayGroup(gameDayId));
}
