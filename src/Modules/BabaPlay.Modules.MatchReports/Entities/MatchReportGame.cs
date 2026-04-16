using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.MatchReports.Entities;

public class MatchReportGame : BaseEntity
{
    public string MatchReportId { get; set; } = string.Empty;
    public int GameNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public MatchReport? MatchReport { get; set; }
    public ICollection<MatchReportPlayerStat> PlayerStats { get; set; } = new List<MatchReportPlayerStat>();
}