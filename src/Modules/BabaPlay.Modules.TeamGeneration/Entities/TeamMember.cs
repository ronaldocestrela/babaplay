using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.TeamGeneration.Entities;

public class TeamMember : BaseEntity
{
    public string TeamId { get; set; } = string.Empty;
    public string AssociateId { get; set; } = string.Empty;
    public int Order { get; set; }

    public Team? Team { get; set; }
}
