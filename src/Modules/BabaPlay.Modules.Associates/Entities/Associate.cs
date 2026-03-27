using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Associates.Entities;

public class Associate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? UserId { get; set; }

    /// <summary>When false, the linked user cannot sign in as an associate.</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<AssociatePosition> Positions { get; set; } = new List<AssociatePosition>();
}
