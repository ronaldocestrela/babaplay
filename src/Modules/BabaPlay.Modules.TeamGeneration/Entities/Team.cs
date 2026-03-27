using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.TeamGeneration.Entities;

public class Team : BaseEntity
{
    public string? SessionId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
}
