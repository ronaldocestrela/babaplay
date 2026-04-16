using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.MatchReports.Entities;

public class MatchReport : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public MatchReportStatus Status { get; set; } = MatchReportStatus.Draft;
    public DateTime? FinalizedAt { get; set; }
    public string? FinalizedByUserId { get; set; }

    public ICollection<MatchReportGame> Games { get; set; } = new List<MatchReportGame>();
}

public enum MatchReportStatus
{
    Draft = 0,
    Finalized = 1,
}