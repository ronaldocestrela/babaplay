using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Associations.Entities;

public class Association : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Regulation { get; set; }

    /// <summary>Target squad size when generating teams from check-ins (e.g. 5 futsal, 11 football).</summary>
    public int PlayersPerTeam { get; set; } = 5;
}
