using BabaPlay.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BabaPlay.Infrastructure.Hubs;

[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class MatchHub : Hub
{
    public const string MatchEventCreatedEvent = "matchEventCreated";
    public const string MatchEventUpdatedEvent = "matchEventUpdated";
    public const string MatchEventDeletedEvent = "matchEventDeleted";

    public static string MatchGroup(Guid matchId) => $"match:{matchId}";

    public Task JoinMatch(Guid matchId)
        => Groups.AddToGroupAsync(Context.ConnectionId, MatchGroup(matchId));

    public Task LeaveMatch(Guid matchId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, MatchGroup(matchId));
}
