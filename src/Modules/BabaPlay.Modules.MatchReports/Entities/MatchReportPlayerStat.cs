using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.MatchReports.Entities;

public class MatchReportPlayerStat : BaseEntity
{
    public string MatchReportGameId { get; set; } = string.Empty;
    public string AssociateId { get; set; } = string.Empty;
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public string? Observations { get; set; }

    public MatchReportGame? MatchReportGame { get; set; }
}