namespace BabaPlay.Application.DTOs;

public sealed record UserNotificationPreferencesResponse(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    bool PushEnabled,
    bool CheckinEnabled,
    bool MatchEnabled,
    bool MatchEventEnabled,
    bool GameDayEnabled,
    bool IsActive);
