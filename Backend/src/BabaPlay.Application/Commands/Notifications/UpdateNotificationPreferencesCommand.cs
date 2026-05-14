using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Notifications;

public sealed record UpdateNotificationPreferencesCommand(
    Guid UserId,
    bool PushEnabled,
    bool CheckinEnabled,
    bool MatchEnabled,
    bool MatchEventEnabled,
    bool GameDayEnabled) : ICommand<Result<UserNotificationPreferencesResponse>>;
