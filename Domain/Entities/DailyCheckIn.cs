namespace Domain.Entities;

public class DailyCheckIn : BaseEntity
{
    /// <summary>
    /// The associado that performed the check-in.
    /// </summary>
    public required string AssociadoId { get; set; }

    /// <summary>
    /// The date that this check-in is valid for (UTC date). Only one check-in per associado per date is allowed.
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// UTC timestamp when the check-in was registered.
    /// </summary>
    public required DateTime CheckInAtUtc { get; set; }

    /// <summary>
    /// Navigation property for convenience.
    /// </summary>
    public virtual Associado? Associado { get; set; }
}
