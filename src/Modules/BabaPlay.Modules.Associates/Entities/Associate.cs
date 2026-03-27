using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Associates.Entities;

public class Associate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? UserId { get; set; }

    public ICollection<AssociatePosition> Positions { get; set; } = new List<AssociatePosition>();
}
