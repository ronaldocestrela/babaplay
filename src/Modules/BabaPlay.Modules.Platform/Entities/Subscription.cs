using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Platform.Entities;

public class Subscription : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public virtual Tenant? Tenant { get; set; }
    public virtual Plan? Plan { get; set; }
}

public enum SubscriptionStatus
{
    Active = 0,
    Suspended = 1,
    Cancelled = 2
}
