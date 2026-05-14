using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

public sealed class UserNotificationPreferences : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public bool PushEnabled { get; private set; }
    public bool CheckinEnabled { get; private set; }
    public bool MatchEnabled { get; private set; }
    public bool MatchEventEnabled { get; private set; }
    public bool GameDayEnabled { get; private set; }
    public bool IsActive { get; private set; }

    private UserNotificationPreferences() { }

    public static UserNotificationPreferences CreateDefault(Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (userId == Guid.Empty)
            throw new ValidationException("UserId", "UserId is required.");

        return new UserNotificationPreferences
        {
            TenantId = tenantId,
            UserId = userId,
            PushEnabled = true,
            CheckinEnabled = true,
            MatchEnabled = true,
            MatchEventEnabled = true,
            GameDayEnabled = true,
            IsActive = true,
        };
    }

    public void Update(bool pushEnabled, bool checkinEnabled, bool matchEnabled, bool matchEventEnabled, bool gameDayEnabled)
    {
        PushEnabled = pushEnabled;
        CheckinEnabled = checkinEnabled;
        MatchEnabled = matchEnabled;
        MatchEventEnabled = matchEventEnabled;
        GameDayEnabled = gameDayEnabled;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }
}
