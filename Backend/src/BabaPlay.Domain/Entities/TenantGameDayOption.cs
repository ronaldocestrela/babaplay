using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

public sealed class TenantGameDayOption : EntityBase
{
    public Guid TenantId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly LocalStartTime { get; private set; }
    public bool IsActive { get; private set; }

    private TenantGameDayOption() { }

    public static TenantGameDayOption Create(Guid tenantId, DayOfWeek dayOfWeek, TimeOnly localStartTime)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        return new TenantGameDayOption
        {
            TenantId = tenantId,
            DayOfWeek = dayOfWeek,
            LocalStartTime = localStartTime,
            IsActive = true,
        };
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
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
