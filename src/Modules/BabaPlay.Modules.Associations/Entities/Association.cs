using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Associations.Entities;

public class Association : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Regulation { get; set; }
}
