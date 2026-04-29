using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.CheckIns.Entities;

public class CheckInSession : BaseEntity
{
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public string? CreatedByUserId { get; set; }

    /// <summary>Computed persisted column (CAST(StartedAt AS DATE)). Supports the daily unique index.</summary>
    public DateTime? StartedAtDateUtc { get; private set; }

    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
}
