using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Associates.Entities;

public class AssociatePosition : BaseEntity
{
    public string AssociateId { get; set; } = string.Empty;
    public string PositionId { get; set; } = string.Empty;

    public Associate? Associate { get; set; }
    public Position? Position { get; set; }
}
